using System;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Ensage;
using Ensage.Common.Menu;
using Ensage.Common.Objects.UtilityObjects;
using Ensage.Common.Threading;
using Ensage.SDK.Extensions;
using Ensage.SDK.Handlers;
using Ensage.SDK.Helpers;

namespace VisagePlus.Features
{
    internal class FamiliarsCombo
    {
        private Config Config { get; }

        private MenuManager Menu { get; }

        private VisagePlus Main { get; }

        private Extensions Extensions { get; }

        private MultiSleeper MultiSleeper { get; }

        private Unit Owner { get; }

        private TaskHandler Handler { get; }

        public FamiliarsCombo(Config config)
        {
            Config = config;
            Menu = config.Menu;
            Main = config.Main;
            Extensions = config.Extensions;
            MultiSleeper = config.MultiSleeper;
            Owner = config.Main.Context.Owner;

            config.Menu.ComboKeyItem.PropertyChanged += ComboChanged;
            config.Menu.FamiliarsLockItem.PropertyChanged += FamiliarsLockChanged;

            if (config.Menu.FamiliarsLockItem)
            {
                config.Menu.FamiliarsLockItem.Item.SetValue(new KeyBind(config.Menu.FamiliarsLockItem.Value, KeyBindType.Toggle));
            }

            Handler = UpdateManager.Run(ExecuteAsync, true, false);
        }

        public void Dispose()
        {
            if (Menu.ComboKeyItem)
            {
                Handler?.Cancel();
            }

            Menu.FamiliarsLockItem.PropertyChanged -= FamiliarsLockChanged;
            Menu.ComboKeyItem.PropertyChanged -= ComboChanged;
        }

        private void FamiliarsLockChanged(object sender, PropertyChangedEventArgs e)
        {
            if (Menu.FamiliarsLockItem)
            {
                Menu.FollowKeyItem.Item.SetValue(new KeyBind(Menu.FollowKeyItem.Value, KeyBindType.Toggle));
                Menu.LastHitItem.Item.SetValue(new KeyBind(Menu.LastHitItem.Value, KeyBindType.Toggle));
            }

            if (Handler.IsRunning)
            {
                Handler?.Cancel();
            }

            if (Menu.FamiliarsLockItem)
            {
                Handler.RunAsync();
            }
            else
            {
                Handler?.Cancel();
            }
        }

        private void ComboChanged(object sender, PropertyChangedEventArgs e)
        {
            if (Menu.FamiliarsLockItem)
            {
                return;
            }

            if (Menu.ComboKeyItem)
            {
                Handler.RunAsync();
            }
            else
            {
                Handler?.Cancel();
            }
        }

        private async Task ExecuteAsync(CancellationToken token)
        {
            try
            {
                if (Game.IsPaused)
                {
                    return;
                }

                var familiars =
                    EntityManager<Unit>.Entities.Where(x =>
                                                       x.IsValid &&
                                                       x.IsAlive &&
                                                       x.IsControllable &&
                                                       !x.IsStunned() &&
                                                       x.IsAlly(Owner) &&
                                                       x.NetworkName == "CDOTA_Unit_VisageFamiliar").ToList();

                Hero target = null;
                if (Menu.FamiliarsLockItem)
                {
                    target = Config.UpdateMode.FamiliarTarget;
                }
                else
                {
                    target = Config.UpdateMode.Target;
                }

                foreach (var familiar in familiars)
                {
                    if (target != null)
                    {
                        var graveChillDebuff = target.HasModifier(Main.GraveChill.TargetModifierName);
                        var stunDebuff = target.Modifiers.Any(x => x.IsValid && x.IsStunDebuff && x.RemainingTime > 0.5f);
                        var hexDebuff = target.Modifiers.Any(x => x.IsValid  && x.Name == "modifier_sheepstick_debuff" && x.RemainingTime > 0.5f);
                        var atosDebuff = target.Modifiers.Any(x => x.IsValid && x.Name == "modifier_rod_of_atos_debuff" && x.RemainingTime > 0.5f);
                        var familiarsStoneForm = familiar.GetAbilityById(AbilityId.visage_summon_familiars_stone_form);

                        if (Extensions.Cancel(target))
                        {
                            // FamiliarsStoneForm
                            if (Menu.AbilityToggler.Value.IsEnabled(familiarsStoneForm.Name)
                                && Extensions.CanBeCasted(familiarsStoneForm, familiar)
                                && familiar.Distance2D(target) <= 100
                                && !graveChillDebuff && !stunDebuff && !hexDebuff && !atosDebuff
                                && !MultiSleeper.Sleeping("FamiliarsStoneForm"))
                            {
                                Extensions.UseAbility(familiarsStoneForm, familiar);
                                MultiSleeper.Sleep(familiarsStoneForm.GetAbilitySpecialData("stun_duration") * 1000 - 200, "FamiliarsStoneForm");
                                await Await.Delay(Extensions.GetDelay);
                            }
                        }

                        if (target.IsInvulnerable() || target.IsAttackImmune())
                        {
                            Extensions.Move(familiar, target.Position);
                        }
                        else 
                        
                        if (!Menu.AbilityToggler.Value.IsEnabled(familiarsStoneForm.Name) 
                            || target.IsMagicImmune() 
                            || !Extensions.CanBeCasted(familiarsStoneForm, familiar)
                            || graveChillDebuff || stunDebuff || hexDebuff || atosDebuff)
                        {
                            Extensions.Attack(familiar, target);
                        }
                        else
                        {
                            Extensions.Move(familiar, target.Position);
                        }
                    }
                    else
                    {
                        if (Menu.FamiliarsFollowItem)
                        {
                            Extensions.Move(familiar, Game.MousePosition);
                        }
                        else
                        {
                            Extensions.Follow(familiar, Owner);
                        }
                    }
                }
            }
            catch (TaskCanceledException)
            {
                // canceled
            }
            catch (Exception e)
            {
                Main.Log.Error(e);
            }
        }
    }
}
