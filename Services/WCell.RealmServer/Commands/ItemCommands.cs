using System;
using System.Linq;
using WCell.Constants.Items;
using WCell.Constants.Updates;
using WCell.Intercommunication.DataTypes;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Items;
using WCell.RealmServer.Items.Enchanting;
using WCell.RealmServer.Lang;
using WCell.Util.Commands;

namespace WCell.RealmServer.Commands
{
    /// <summary>
    /// TODO: Enable selection of items and calling of commands on it
    /// </summary>
    public class InventoryCommand : RealmServerCommand
    {
        protected InventoryCommand() { }

        protected override void Initialize()
        {
            Init("Inv", "Item");
            EnglishDescription = "Used for manipulation of Items and Inventory.";
        }

        public override ObjectTypeCustom TargetTypes
        {
            get { return ObjectTypeCustom.Player; }
        }

        #region Purge

        public class PurgeInvCommand : SubCommand
        {
            protected PurgeInvCommand() { }

            protected override void Initialize()
            {
                Init("PurgeAll", "Clear");
                EnglishParamInfo = "";
                EnglishDescription = "Removes *all* Items from the target Character. Use carefully!";
            }

            public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
            {
                ((Character)trigger.Args.Target).Inventory.Purge();
            }
        }

        #endregion Purge

        #region Strip

        public class StripCommand : SubCommand
        {
            protected StripCommand() { }

            protected override void Initialize()
            {
                Init("Strip");
                EnglishParamInfo = "";
                EnglishDescription = "Strips the Target naked and puts all the equipment into his/her bag (given there is enough space)";
            }

            public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
            {
                ((Character)trigger.Args.Target).Inventory.Strip();
            }
        }

        #endregion Strip

        #region Add

        public class ItemAddCommand : SubCommand
        {
            protected ItemAddCommand() { }

            protected override void Initialize()
            {
                Init("Add", "Create");
                EnglishParamInfo = "[-ea] <itemid> [<amount> [<stacks>]]";
                EnglishDescription = "Adds the given amount of stacks (default: 1) of the given amount " +
                    "of the given item to your backpack (if there is space left). " +
                     "-a switch auto-equips, -e switch only adds if not already present.";
            }

            public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
            {
                var mods = trigger.Text.NextModifiers();
                var entry = trigger.Text.NextEnum(ItemId.None);
                var templ = ItemMgr.GetTemplate(entry);

                if (templ == null)
                {
                    trigger.Reply("Invalid ItemId.");
                    return;
                }

                if (templ.IsCharter)
                {
                    trigger.Reply("Charters cannot be added by command.");
                    return;
                }

                var amount = trigger.Text.NextInt(1);
                var stacks = trigger.Text.NextUInt(1);
                var ensure = mods.Contains("e");
                var autoEquip = mods.Contains("a");

                for (var i = 0; i < stacks; i++)
                {
                    if (!AddItem((Character)trigger.Args.Target, templ, amount, autoEquip, ensure))
                    {
                        break;
                    }
                }
                //trigger.Reply("{0}/{1} stacks of {2} created{3}", x, stacks, templ, err == InventoryError.OK ? "." : ": " + err);
            }

            public static bool AddItem(Character chr, ItemTemplate templ, int amount, bool autoEquip, bool ensureOnly)
            {
                var actualAmount = amount;
                var inv = chr.Inventory;

                InventoryError err;
                if (ensureOnly)
                {
                    // only add if necessary
                    err = inv.Ensure(templ, amount, autoEquip);
                }
                else
                {
                    if (autoEquip)
                    {
                        // auto-equip
                        var slot = inv.GetEquipSlot(templ, true);
                        if (slot == InventorySlot.Invalid)
                        {
                            err = InventoryError.INVENTORY_FULL;
                        }
                        else
                        {
                            err = inv.TryAdd(templ, slot);
                        }
                    }
                    else
                    {
                        // just add
                        err = inv.TryAdd(templ, ref amount);
                    }
                }

                if (err != InventoryError.OK || amount < actualAmount)
                {
                    // something went wrong
                    if (err != InventoryError.OK)
                    {
                        ItemHandler.SendInventoryError(chr.Client, null, null, err);
                    }
                    return false;
                }
                return true;
            }
        }

        #endregion Add

        #region CreateSet

        public class CreateSetCommand : SubCommand
        {
            protected CreateSetCommand() { }

            protected override void Initialize()
            {
                Init("CreateSet", "AddSet");
                EnglishParamInfo = "<setId>";
                EnglishDescription = "Creates all items of the set with the given id and puts them into a new bag which gets auto-equipped (requires free bag-slot)";
            }

            public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
            {
                var id = trigger.Text.NextEnum(ItemSetId.TheHighlandersWill);
                bool created = ItemSet.CreateSet(((Character)trigger.Args.Target), id);
                trigger.Reply("ItemSet {0}created.", created ? "" : "could not be ");
                if (!created)
                {
                    trigger.Reply("Make sure that the id is valid and you have a free bag slot.");
                }
            }
        }

        #endregion CreateSet

        #region Find

        public class FindItemCommand : SubCommand
        {
            protected FindItemCommand() { }

            private const int MIN_SEARCH_CHARS = 3;

            protected override void Initialize()
            {
                Init("Find");
                EnglishParamInfo = "[-l <locale>] <search text>";
                EnglishDescription = "Search for Items whose name contains the specified text. " +
                    "If no locale specified, it will use the User's default.";
            }

            public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
            {
                var locale = trigger.Args.User != null ? trigger.Args.User.Locale : RealmServerConfiguration.DefaultLocale;

                var mod = trigger.Text.NextModifiers();
                if (mod == "l")
                {
                    locale = trigger.Text.NextEnum(locale);
                }

                var itemName = trigger.Text.Remainder.Trim();

                if (itemName.Length >= MIN_SEARCH_CHARS)
                {
                    var itemsFound = ItemMgr.Templates.Where(item => item != null &&
                        (item.Names.Localize(locale).IndexOf(itemName, StringComparison.InvariantCultureIgnoreCase) >= 0));

                    var i = 0;
                    foreach (var item in itemsFound)
                    {
                        i++;
                        trigger.Reply("{3}. {0} - ({1} ({2}))", item.DefaultName, item.ItemId, item.Id, i);
                    }
                    trigger.Reply("{0} Items found.", itemsFound.Count());
                }
                else
                {
                    trigger.Reply("Argument itemName requires at least {0} characters", MIN_SEARCH_CHARS);
                }
            }
        }

        #endregion Find

        #region Enchant

        public class EnchantItemCommand : SubCommand
        {
            protected EnchantItemCommand() { }

            protected override void Initialize()
            {
                Init("Enchant", "Ench");
                EnglishParamInfo = "[-l <text>]|(<slot> <enchantid> [<EnchantSlot>])";
                EnglishDescription = "Enchants the Item at the given slot. Alternatively the -l switch lists all enchants which match the given text";
            }

            public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
            {
                var mod = trigger.Text.NextModifiers();
                if (mod == "l")
                {
                    var text = trigger.Text.Remainder.Trim();
                    var enchants = EnchantMgr.EnchantmentEntryReader.Entries.Values.Where(
                        enchant => enchant.Description.IndexOf(text, StringComparison.InvariantCultureIgnoreCase) > -1);

                    trigger.Reply("{0} Enchantments found", enchants.Count());
                    var i = 0;
                    foreach (var enchant in enchants)
                    {
                        i++;
                        trigger.Reply("{2}. {0} - {1})", enchant.Id, enchant.Description, i);
                    }
                }
                else
                {
                    var slot = trigger.Text.NextEnum(InventorySlot.Invalid);
                    if (slot == InventorySlot.Invalid)
                    {
                        trigger.Reply("Invalid slot.");
                    }
                    else
                    {
                        var item = ((Character)trigger.Args.Target).Inventory[slot];
                        if (item == null)
                        {
                            trigger.Reply("There is no Item in slot " + slot);
                        }
                        else
                        {
                            var id = trigger.Text.NextUInt();
                            var enchantSlot = trigger.Text.NextEnum(EnchantSlot.Permanent);
                            var enchant = EnchantMgr.GetEnchantmentEntry(id);
                            if (enchant == null)
                            {
                                trigger.Reply("Invalid EnchantId: " + id);
                            }
                            else
                            {
                                item.ApplyEnchant(enchant, enchantSlot, 0, 0, true);
                            }
                        }
                    }
                }
            }
        }

        #endregion Enchant

        #region Destroy

        public class DestroyItemCommand : SubCommand
        {
            protected DestroyItemCommand() { }

            protected override void Initialize()
            {
                Init("Destroy", "Remove", "Delete", "Del");
                EnglishParamInfo = "";
                EnglishDescription = "Removes the Item at the given Slot from the target Character.";
            }

            public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
            {
                var slot = trigger.Text.NextEnum(InventorySlot.Invalid);
                if (slot == InventorySlot.Invalid)
                {
                    trigger.Reply("Invalid slot.");
                }
                else
                {
                    var item = ((Character)trigger.Args.Target).Inventory[slot];
                    if (item == null)
                    {
                        trigger.Reply("There is no Item in slot " + slot);
                    }
                    else
                    {
                        trigger.Reply("Destroyed: " + item.Name);
                        item.Destroy();
                    }
                }
            }
        }

        #endregion Destroy

        #region Get/Set/Call

        public class ItemSetCommand : SubCommand
        {
            protected ItemSetCommand() { }

            public override RoleStatus DefaultRequiredStatus
            {
                get { return RoleStatus.Admin; }
            }

            protected override void Initialize()
            {
                Init("Set", "S");
                EnglishParamInfo = "<invslot> <some.prop> <someValue>";
                EnglishDescription = "Sets properties on the Item in the given slot";
            }

            public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
            {
                var slot = trigger.Text.NextEnum(InventorySlot.Invalid);
                if (slot == InventorySlot.Invalid)
                {
                    trigger.Reply("Invalid slot.");
                }
                else
                {
                    var item = ((Character)trigger.Args.Target).Inventory[slot];
                    if (item == null)
                    {
                        trigger.Reply("There is no Item in slot " + slot);
                    }
                    else
                    {
                        SetCommand.Set(trigger, item);
                    }
                }
            }
        }

        public class ItemModCommand : SubCommand
        {
            protected ItemModCommand() { }

            public override RoleStatus DefaultRequiredStatus
            {
                get { return RoleStatus.Admin; }
            }

            protected override void Initialize()
            {
                Init("Mod", "M");
                EnglishParamInfo = @"<invslot> <some.prop> (+-|&^*\...) <expr>";
                EnglishDescription = "Mods properties on the Item in the given slot";
            }

            public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
            {
                var slot = trigger.Text.NextEnum(InventorySlot.Invalid);
                if (slot == InventorySlot.Invalid)
                {
                    trigger.Reply("Invalid slot.");
                }
                else
                {
                    var item = ((Character)trigger.Args.Target).Inventory[slot];
                    if (item == null)
                    {
                        trigger.Reply("There is no Item in slot " + slot);
                    }
                    else
                    {
                        ModPropCommand.ModProp(trigger, item);
                    }
                }
            }
        }

        public class ItemGetCommand : SubCommand
        {
            protected ItemGetCommand() { }

            public override RoleStatus DefaultRequiredStatus
            {
                get { return RoleStatus.Admin; }
            }

            protected override void Initialize()
            {
                Init("Get", "G");
                EnglishParamInfo = "<invslot> <some.prop>";
                EnglishDescription = "Gets properties on the Item in the given slot";
            }

            public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
            {
                var slot = trigger.Text.NextEnum(InventorySlot.Invalid);
                if (slot == InventorySlot.Invalid)
                {
                    trigger.Reply("Invalid slot.");
                }
                else
                {
                    var item = ((Character)trigger.Args.Target).Inventory[slot];
                    if (item == null)
                    {
                        trigger.Reply("There is no Item in slot " + slot);
                    }
                    else
                    {
                        GetCommand.GetAndReply(trigger, item);
                    }
                }
            }

            public override object Eval(CmdTrigger<RealmServerCmdArgs> trigger)
            {
                var slot = trigger.Text.NextEnum(InventorySlot.Invalid);
                if (slot != InventorySlot.Invalid)
                {
                    return ((Character)trigger.Args.Target).Inventory[slot];
                }
                return null;
            }
        }

        public class ItemCallCommand : SubCommand
        {
            protected ItemCallCommand() { }

            public override RoleStatus DefaultRequiredStatus
            {
                get
                {
                    return RoleStatus.Admin;
                }
            }

            protected override void Initialize()
            {
                Init("Call", "C");
                EnglishParamInfo = "<invslot> <call-args>";
                EnglishDescription = "Calls the given method with parameters on the Item in the given slot.";
            }

            public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
            {
                var slot = trigger.Text.NextEnum(InventorySlot.Invalid);
                if (slot == InventorySlot.Invalid)
                {
                    trigger.Reply("Invalid slot.");
                }
                else
                {
                    var item = ((Character)trigger.Args.Target).Inventory[slot];
                    if (item == null)
                    {
                        trigger.Reply("There is no Item in slot " + slot);
                    }
                    else
                    {
                        CallCommand.Call(trigger, item);
                    }
                }
            }
        }

        #endregion Get/Set/Call
    }

    #region DumpInventory

    public class DumpInventoryCommand : RealmServerCommand
    {
        protected DumpInventoryCommand() { }

        protected override void Initialize()
        {
            Init("DumpInventory", "DumpInv");
            EnglishDescription = "Dumps all items that are currently in the backpack";
        }

        public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
        {
            trigger.Reply("{0}'s BackPack:", trigger.Args.Target);
            var inv = ((Character)trigger.Args.Target).Inventory;
            for (var i = inv.BackPack.Offset; i <= inv.BackPack.End; i++)
            {
                trigger.Reply("{0}: {1}", i, inv.Items[i]);
            }
        }

        public override ObjectTypeCustom TargetTypes
        {
            get
            {
                return ObjectTypeCustom.Player;
            }
        }
    }

    #endregion DumpInventory
}