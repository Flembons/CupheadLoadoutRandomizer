using Blender.Content;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace LoadoutRandomizer
{
    public class LoadoutRandomizer
    {
        public List<string> customWeapons;
        public List<string> customCharms;
        public List<string> customSupers;

        public void Init()
        {
            On.MapEquipUICard.HandleInputFront += HandleInputFront;
            On.AbstractEquipUI.Awake += Awake;
            On.LevelGameOverGUI.outforequip_cr += outforequip_cr;
        }

        public void randomizeLoadout(PlayerId playerId)
        {
            // Get the player's current loadout
            var loadout = PlayerData.Data.Loadouts.GetPlayerLoadout(playerId);

            // Get a list of all custom weapons loaded by Blender
            customWeapons = EquipRegistries.Weapons.GetNames();

            // Create a list of all unlocked weapons
            List<Weapon> weapons = new List<Weapon>(PlayerData.Data.inventories.GetPlayer(playerId)._weapons);

            // Remove duplicates from the weapons list (there duplicates sometimes in the weapons list and I'm not sure why)
            weapons = weapons.Distinct<Weapon>().ToList();

            // Remove any weapons that aren't used for levels
            // This also removes any uninstalled modded weapons, because those linger in the save file after uninstalling mods that add new weapons
            weapons.RemoveAll(weaponsToIgnore);

            // Select a random primary and secondary weapon (duplicates not allowed)
            int index;
            if (weapons.Count > 0)
            {
                index = UnityEngine.Random.Range(0, weapons.Count);
                loadout.primaryWeapon = weapons[index];
                weapons.RemoveAt(index);
            }
            if (weapons.Count > 0)
            {
                index = UnityEngine.Random.Range(0, weapons.Count);
                loadout.secondaryWeapon = weapons[index];
            }

            // Get a list of all custom charms loaded by Blender
            customCharms = EquipRegistries.Charms.GetNames();

            // Select a random Charm if the player has any unlocked
            List<Charm> charms = new List<Charm>(PlayerData.Data.inventories.GetPlayer(playerId)._charms);

            // Remove duplicates from the charms list (there's duplicates sometimes in the weapons list and I'm not sure why)
            charms = charms.Distinct<Charm>().ToList();

            // Remove all unimplemented charms and any charms that aren't in the correct naming format
            charms.RemoveAll(charmsToIgnore);

            if (charms.Count > 0)
            {
                index = UnityEngine.Random.Range(0, charms.Count);
                loadout.charm = charms[index];
            }

            // Get a list of all custom supers loaded by Blender
            customSupers = EquipRegistries.Supers.GetNames();

            // Select a random Super if the player has any unlocked
            List<Super> supers = new List<Super>(PlayerData.Data.inventories.GetPlayer(playerId)._supers);

            // Remove duplicates from the supers list
            supers = supers.Distinct<Super>().ToList();

            // Remove all supers that are not implemented
            supers.RemoveAll(supersToIgnore);

            if (supers.Count > 0)
            {
                // Remove all Chalice supers if the charm isn't Ms. Chalice
                if (loadout.charm != Charm.charm_chalice)
                {
                    supers.Remove(Super.level_super_chalice_vert_beam);
                    supers.Remove(Super.level_super_chalice_shield);
                }
                // Remove all the Cuphead supers if Chalice is the current charm
                else
                {
                    supers.Remove(Super.level_super_beam);
                    supers.Remove(Super.level_super_invincible);
                }

                index = UnityEngine.Random.Range(0, supers.Count);
                loadout.super = supers[index];
            }
        }

        // Cuphead's charm list contains unimplemented charms that should be ignored unless they've been reimplemented by another mod
        public bool charmsToIgnore (Charm charm)
        {
            string[] unimplementedCharms = { "charm_pit_saver", "charm_directional_dash", "charm_EX", "charm_float" };

            if (unimplementedCharms.Contains(charm.ToString()))
            {
                return !customCharms.Contains(charm.ToString());
            }
            return !charm.ToString().StartsWith("charm_");
        }

        // Cuphead's weapon list contains unimplemented weapons that should be ignored unless they've been reimplemented by another mod
        public bool weaponsToIgnore (Weapon weapon)
        {
            string[] unimplementedWeapons = { "level_weapon_arc", "level_weapon_accuracy", "level_weapon_exploder",
                "level_weapon_pushback", "level_weapon_splitter", "level_weapon_firecracker", "level_weapon_firecrackerB" };

            if (unimplementedWeapons.Contains(weapon.ToString()))
            {
                return !customWeapons.Contains(weapon.ToString());
            }

            return !weapon.ToString().StartsWith("level_weapon_");
        }

        // Cuphead's super list contains unimplemented supers that should be ignored unless they've been reimplemented by another mod
        public bool supersToIgnore (Super super)
        {
            // I'm not exactly sure what these supers are, but they don't work when selected
            string[] unimplementedSupers = { "level_super_chalice_iii", "level_super_chalice_bounce" };

            if (unimplementedSupers.Contains(super.ToString()))
            {
                return !customSupers.Contains(super.ToString());
            }

            return !super.ToString().StartsWith("level_super_");
        }

        public void HandleInputFront(On.MapEquipUICard.orig_HandleInputFront orig, MapEquipUICard self)
        {
            if (self.playerInput.GetButtonDown(14))
            {
                self.Close();
                return;
            }
            if (self.playerInput.GetButtonDown(18))
            {
                self.front.ChangeSelection(-1);
                return;
            }
            if (self.playerInput.GetButtonDown(20))
            {
                self.front.ChangeSelection(1);
                return;
            }

            // The only new bit of code here. Pressing the shoot button will randomize your loadout then refresh the UI to reflect the new loadout
            if (self.playerInput.GetButtonDown(3))
            {
                randomizeLoadout(self.playerID);
                var cardFronts = GameObject.FindObjectsOfType<MapEquipUICardFront>();
                foreach (var cardFront in cardFronts)
                {
                    cardFront.Refresh();
                    cardFront.ChangeSelection(0);
                }
            }
            if (self.front.checkListSelected)
            {
                if (self.playerInput.GetButtonDown(13))
                {
                    int index = 0;
                    if (PlayerData.Data.CurrentMap == Scenes.scene_map_world_2)
                    {
                        index = 1;
                    }
                    else if (PlayerData.Data.CurrentMap == Scenes.scene_map_world_3)
                    {
                        index = 2;
                    }
                    else if (PlayerData.Data.CurrentMap == Scenes.scene_map_world_4)
                    {
                        index = 3;
                    }
                    else if (PlayerData.Data.CurrentMap == Scenes.scene_map_world_DLC)
                    {
                        index = 4;
                    }
                    self.checkList.SetCursorPosition(index, true);
                    self.RotateToCheckList();
                    return;
                }
            }
            else
            {
                if (self.playerInput.GetButtonDown(13))
                {
                    self.RotateToBackSelect(self.front.Slot);
                    return;
                }
                if (self.playerInput.GetButtonDown(15))
                {
                    self.front.Unequip();
                    return;
                }
            }
        }

        protected void Awake(On.AbstractEquipUI.orig_Awake orig, AbstractEquipUI self)
        {
            orig(self);

            // Find the EquipUI transform
            var helpText = self.transform.Find("Help (2)");

            // Create a new space between buttons
            var spacing = UnityEngine.Object.Instantiate(helpText.GetChild(5).gameObject, helpText);

            // Create the randomize text
            var text = UnityEngine.Object.Instantiate(helpText.GetChild(6).gameObject, helpText);

            // Create the Shoot button glyph
            var glyph = UnityEngine.Object.Instantiate(helpText.GetChild(7).gameObject, helpText);

            // Cuphead's UI uses a layout, so these new objects need to be the first siblings so they're in the correct place
            spacing.transform.SetSiblingIndex(0);

            // Set the glyph's associated button to the Shoot button
            glyph.gameObject.GetComponent<CupheadGlyph>().button = CupheadButton.Shoot;
            glyph.transform.SetSiblingIndex(0);

            // Change the button's text to Randomize
            text.gameObject.GetComponent<Text>().text = "RANDOMIZE";
            text.transform.SetSiblingIndex(0);
        }

        private IEnumerator outforequip_cr(On.LevelGameOverGUI.orig_outforequip_cr orig, LevelGameOverGUI self)
        {
            self.state = LevelGameOverGUI.State.Init;
            self.equipUI.gameObject.SetActive(true);
            self.equipUI.Activate();
            yield return self.TweenValue(1f, 0f, 0.3f, EaseUtils.EaseType.easeOutCubic, new AbstractMonoBehaviour.TweenUpdateHandler(self.SetCardValueEquipSwap));

            // New code here: Changes the EquipUI's text to Randomize because it resets to "Unequip" otherwise
            self.equipUI.transform.Find("Help (2)/Text (2)(Clone)").gameObject.GetComponent<Text>().text = "RANDOMIZE";

            yield break;
        }
    }
}