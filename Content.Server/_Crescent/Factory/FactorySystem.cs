using Content.Shared.Inventory;
using JetBrains.Annotations;
using Robust.Shared.Physics.Events;
using Robust.Shared.Physics.Systems;
using Content.Server.Station.Systems;
using Content.Server.Power.Components;
using Content.Server.Factory.Components;
using Content.Shared.Physics;
using Robust.Shared.Physics.Collision.Shapes;
using Robust.Shared.Physics.Components;
using Content.Server.DeviceLinking.Events;
using Content.Server.DeviceLinking.Systems;
using System.Linq;
using Content.Server.Stack;
using Content.Shared.Stacks;
using Robust.Shared.Map;

namespace Content.Server.Factory.EntitySystems
{


    [UsedImplicitly]
    public sealed partial class FactorySystem : EntitySystem
    {
        [Dependency] private readonly FixtureSystem _fixtures = default!;
        [Dependency] private readonly DeviceLinkSystem _signalSystem = default!;
        [Dependency] private readonly StackSystem _stacks = default!;

        const string FactoryFixture = "FactoryFixture";


        private float _internalClock = 0f;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<FactoryComponent, ComponentInit>(OnInit);
            SubscribeLocalEvent<FactoryComponent, ComponentShutdown>(OnDel);


            SubscribeLocalEvent<FactoryComponent, StartCollideEvent>(OnInsertion);
            SubscribeLocalEvent<FactoryComponent, EndCollideEvent>(OnRemoval);

            SubscribeLocalEvent<FactoryComponent, SignalReceivedEvent>(OnSignalReceived);
            SubscribeLocalEvent<FactoryComponent, PowerChangedEvent>(OnPowerChanged);
        }

        private void OnInit(EntityUid uid, FactoryComponent component, ComponentInit args)
        {
            _signalSystem.EnsureSinkPorts(uid, component.Toggle);

            if (TryComp<PhysicsComponent>(uid, out var physics))
            {
                var shape = new PolygonShape();
                shape.SetAsBox(0.6f, 0.6f);

                _fixtures.TryCreateFixture(uid, shape, FactoryFixture,
                    collisionLayer: (int) (CollisionGroup.LowImpassable | CollisionGroup.MidImpassable |
                                           CollisionGroup.Impassable), hard: false, body: physics);

            }
        }

        private void OnDel(EntityUid uid, FactoryComponent component, ComponentShutdown args)
        {
            if (!TryComp<PhysicsComponent>(uid, out var physics))
                return;

            _fixtures.DestroyFixture(uid, FactoryFixture, body: physics);
        }

        private void OnInsertion(EntityUid uid, FactoryComponent component, ref StartCollideEvent args)
        {
            component.Inserted.Add(args.OtherEntity);
            component.InsertCount++;
            if (component.InsertCount == 1)
                EnsureComp<ActiveFactoryComponent>(uid);

        }

        private void OnRemoval(EntityUid uid, FactoryComponent component, ref EndCollideEvent args)
        {
            component.Inserted.Remove(args.OtherEntity);
            component.InsertCount--;
            if (component.InsertCount == 0)
                RemComp<ActiveFactoryComponent>(uid);
        }

        private void OnPowerChanged(EntityUid uid, FactoryComponent component, ref PowerChangedEvent args)
        {
            component.Powered = args.Powered;
        }

        private void OnSignalReceived(EntityUid uid, FactoryComponent component, ref SignalReceivedEvent args)
        {
            if(args.Port == component.Toggle)
            {
                component.Active = !component.Active;
            }
        }

        private void Fabricate(EntityUid uid, FactoryComponent comp, FactoryRecipe factoryRecipe)
        {

        }



        public override void Update(float frameTime)
        {
            base.Update(frameTime);
            _internalClock += frameTime;
            if (_internalClock > 0.1f)
            {
                _internalClock = 0f;
                var query = EntityQueryEnumerator<ActiveFactoryComponent, FactoryComponent>();

                while (query.MoveNext(out var uid, out var _, out var comp))
                {
                    /// SETUP
                    TransformComponent? factoryTransform = null;
                    if (!TryComp(uid, out factoryTransform))
                        continue;

                    Dictionary<string, List<EntityUid>> recipeEntities = new();
                    Dictionary<string, int> itemCounts = new();
                    foreach(EntityUid entity in comp.Inserted)
                    {
                        string entityString = entity.ToString();
                        StackComponent? myStack = null;
                        if(TryComp(entity, out myStack))
                        {
                            itemCounts[entityString] += myStack.Count;
                        }
                        else
                            itemCounts[entityString]++;
                        recipeEntities[entityString].Add(entity);
                    }
                    ///

                    /// FABRICATION
                    while(comp.Produced < comp.ProductionCap)
                    {
                        /// RECIPE SEEKING
                        FactoryRecipe? chosenRecipe = null;
                        foreach (var (recipeName, recipePrototype) in comp.Recipes)
                        {
                            bool fulfilled = true;
                            foreach(var (entityRequired, requiredAmount) in recipePrototype.Inputs)
                            {
                                if (itemCounts[entityRequired] < requiredAmount)
                                    fulfilled = false; break;               
                            }
                            if (!fulfilled)
                                continue;
                            chosenRecipe = recipePrototype;
                            break;
                        }
                        if (chosenRecipe is null)
                            break;
                        ///

                        /// RECIPE INPUT
                        comp.Produced++;
                        foreach (var (entityRequired, requiredAmount) in chosenRecipe.Inputs)
                        {
                            var amount = requiredAmount;
                            List<EntityUid> delete = recipeEntities[entityRequired];
                            while (amount > 0 && delete.Count > 0)
                            {
                                EntityUid targetEntity = delete.First();
                                StackComponent? myStack = null;
                                if (TryComp(targetEntity, out myStack))
                                {
                                    var usedAmount = Math.Min(myStack.Count, amount);
                                    if (usedAmount == myStack.Count)
                                        delete.RemoveAt(1);
                                    _stacks.SetCount(targetEntity, myStack.Count - usedAmount, myStack);
                                    amount -= usedAmount;
                                }
                                else
                                {
                                    QueueDel(targetEntity);
                                    amount--;
                                    EntityManager.DeleteEntity(delete.First());
                                    delete.RemoveAt(1);
                                }
                            }
                        }
                        var factoryPos = factoryTransform.LocalPosition;
                        var factoryRot = factoryTransform.LocalRotation;

                        EntityCoordinates targetPos = new EntityCoordinates(uid, factoryPos.X + (float)Math.Sin(factoryRot) * 1.5f, factoryPos.Y + (float)Math.Cos(factoryRot) * 1.5f);
                        /// RECIPE OUTPUT
                        foreach (var (entityRequired, requiredAmount) in chosenRecipe.Outputs)
                        {
                            var amount = requiredAmount;
                            while (amount > 0)
                            {
                                EntityUid newEntity = EntityManager.SpawnAtPosition(entityRequired, targetPos);
                                amount--;
                            }
                        }

                    }
                    /// END OF FABRICATION
                    comp.Produced = 0;

                }
            }
        }

    }
}
