# More Counterplay for Lethal Company enemies

Mod is fully configurable so you can disable or edit counterplays that you don't like.

- Uses [CSync v5.0.1](https://thunderstore.io/c/lethal-company/p/Sigurd/CSync) by [Lordfirespeed](https://github.com/Lordfirespeed) to sync configuration settings between host and clients.

## Supported Enemies

### Jester

<details>
 <summary>Spoiler</summary>
 You can prevent Jester from opening by putting heavy items on top of it.

 ![A Jester carrying a big bolt on its head.](https://i.imgur.com/QcykrPl.jpg)
</details>

<details>
 <summary>Configs</summary>

- `EnableJesterCounterplay` - Add counterplay for Jester.
- `WeightToPreventJester` - Weight of items needed to prevent Jester pop out.

</details>

---

### Turret

<details>
 <summary>Spoiler</summary>

 Turrets can be disabled by cutting their wires with a knife.
 When you hit Turret using knife it will enter berserker mode and after that it will disable permanently!
</details>

<details>
 <summary>Configs</summary>

- `EnableTurretCounterplay` - Toggle Turret counterplay.

</details>

---

### Coilhead

<details>
<summary>Spoiler</summary>

You can cut off a Coilhead's head using a knife. Its head will become a scrap item and can be grabbed and sold, though you must detach it.

<div style="text-align: center;">
 <img alt="A decapitated Coilhead." src="https://i.imgur.com/WtcAkJ9.jpg" width=256>
 <img alt="A 'Coilless Coilhead' on the ground." src="https://i.imgur.com/LvhsWHD.jpg" width=256>
 <h2 style="font-weight: bold; color: firebrick; text-shadow: 0 0 3px black">— [Warning] —</h2>

 > _"They have been known to combust into flames when being dissected or even deactivated, and they carry dangerously high levels of radioactive particles."_ - Sigurd's notes

 **Hint:** _You may find a Coilhead to be less volatile the more kinetic energy it releases when coming to a halt..._

 <details>
 <summary>Spoiler (specific mechanics and configuration):</summary>

 <div style="text-align: left;">

- Coilhead bodies combust upon being decapitated, as their Bestiary entry suggests
  - Can be disabled by toggling the `LoreAccurateCoilheads` setting
  - The range of the explosion damage is determined by the `ExplosionDamageRadius` setting, with the damage itself being set to the value of the `ExplosionDamage` setting
  - Likewise, the `ExplosionKillRadius` setting determines the range around the explosion where it simply kills the player instead of dealing damage to them
- Explosion timer is set to how long the Coilhead has moved since it last stopped, within configurable limits
  - Minimum and maximum time until exploding can be configured via the `MinExplosionTimer` and `MaxExplosionTimer` settings, respectively
- Coilhead's head item is destroyed if its body explodes while it's still attached to its neck
  - Can be disabled by toggling the `ExplosionDestroysHead` setting, but it adds some interesting risk/reward by making players stay close to try and pick up the head before it explodes

 </div>
 </details>
</div>
</details>

<details>
 <summary>Configs</summary>

- `EnableCoilheadCounterplay` - Add counterplay for Coilheads.
- `SpringDurability` - Set Coilhead health points.
- `CoilheadDefaultDamage` - Amount of damage that Coilheads take from any source not specified below.
- `CoilheadKnifeDamage` - Amount of damage that Coilheads take from Knife.
- `CoilheadShovelDamage` - Amount of damage that Coilheads take from Shovel.
- `DropHeadAsScrap` - Enable the Coilhead head scrap item ('Coilless Coilhead') spawning on death.
- `MinHeadValue` - Minimum value of head item.
- `MaxHeadValue` - Maximum value of head item.
- `LoreAccurateCoilheads` - See above for more info.

</details>

---

### Ghost Girl (planned)

<details>
 <summary>Spoiler (WIP)</summary>

 Implements the popular myth involving the Shower furniture item, where taking a shower is said to reduce insanity levels and repel the Ghost Girl, as an actual gameplay mechanic.
</details>

<details>
 <summary>Configs (WIP)</summary>

- `EnableGhostGirlCounterplay` - Add counterplay for the Ghost Girl.

</details>

## Bug Reports / Suggestions

If you come across any issues or mod incompatibilities, or simply have an interesting idea you'd like to see implemented, feel free to drop a message in the [relevant thread](https://discord.com/channels/1168655651455639582/1212542584610881557) in the [Lethal Company Modding Discord server](https://discord.com/invite/lcmod), or [open an issue on GitHub](https://github.com/karyol/More-Counterplay-Mod/issues)!

## Credits

- [Baron Drakula](https://github.com/karyol)
- [pacoito123](https://github.com/pacoito123)
- Society for testing and giving new ideas.

### If you are enjoying my mods please consider supporting me [here](https://ko-fi.com/baron_drakula)
