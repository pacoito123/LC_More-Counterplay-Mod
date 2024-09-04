# Changelog

## 1.3.0

<ul>
 <li>Added <a href="https://thunderstore.io/c/lethal-company/p/Sigurd/CSync">CSync v5</a> as a dependency to ensure config parity between host and clients.</li>
 <li>Added "lore accurate" Coilhead counterplay (configurable):
  <ul>
   <li>
    <blockquote>
    <i>"They have been known to combust into flames when being dissected or even deactivated, and they carry dangerously high levels of radioactive particles."</i> - Sigurd's notes
    </blockquote>
   </li>
   <li>
    <b>Hint:</b> <i>You may find a Coilhead to be less volatile the more kinetic energy it releases when coming to a halt...</i>
   </li>
   <li>
    <details>
     <summary>Spoiler (specific mechanics and configuration):</summary>
     <ul>
      <li>Coilhead bodies now combust upon being decapitated, as their Bestiary entry suggests
       <ul>
        <li>Can be disabled by toggling the <code>LoreAccurateCoilheads</code> setting</li>
        <li>The range of the explosion damage is determined by the <code>ExplosionDamageRadius</code> setting, with the damage itself being set to the value of the <code>ExplosionDamage</code> setting</li>
        <li>Likewise, the <code>ExplosionKillRadius</code> setting determines the range around the explosion where it simply kills the player instead of dealing damage to them</li>
       </ul>
      </li>
      <li>Explosion timer is set to how long the Coilhead has moved since it last stopped, within configurable limits
       <ul>
        <li>Minimum and maximum time until exploding can be configured via the <code>MinExplosionTimer</code> and <code>MaxExplosionTimer</code> settings, respectively</li>
       </ul>
      </li>
      <li>Coilhead's head item is destroyed if its body explodes while it's still attached to its neck
       <ul>
        <li>Can be disabled by toggling the <code>ExplosionDestroysHead</code> setting, but it adds some interesting risk/reward by making players stay close to try and pick up the head before it explodes</li>
       </ul>
      </li>
     </ul>
    </details>
   </li>
  </ul>
 </li>
</ul>

- Coilheads can no longer be decapitated while in their cooldown state (they protect their necks!)
- Coilhead damage should now apply properly for all clients, whereas previously Coilheads would take damage separately even with the same settings
- Renamed Coilhead head item to "Coilless Coilhead"
- Changed "Coilless Coilhead" item sound effects a bit
- Fixed "Coilless Coilhead" item not being properly rotated when held
  - _You may discover it to not be as inanimate as it initially lets on..._
- Fixed several networking issues with the "Coilless Coilhead" item
  - New stuff should also be working reliably in multiplayer
- Reduced AssetBundle size significantly by adding large textures to prefabs at runtime instead of bundling them

## 1.2.3

- Coilhead's head item should no longer vanish from the ship after reloading the save file
- Coilhead's head item now spawns properly attached and facing the right way
- Coilhead's head item no longer floats above the ground when dropped/placed, and is upright
  - This also made it able to be properly placed inside the storage cabinet
- Coilhead's head item scale further reduced to 0.1763
  - Matches the size of the actual head
- Coilhead should no longer get stuck in its moving animation if killed while moving
  - Could happen previously while walking backwards into it while swinging the knife
- Coilhead's neck now wobbles one final time when killed, to indicate its death

## 1.2.2

- Dropped items on Jester should now attach properly (v56 compatibility)
- Items can no longer be placed on Jester while inside the ship
  - Should prevent Jester sneaking under the ship and stealing items in some custom moons
- Fixed error when removing items from the Jester through iteration
- Coilhead's head scale reduced to 0.2

## 1.2.1

- Added Head scrap item
- Coilheads now drop their head on death
- Fixed Coilhead's head disappear only for host bug

## 1.2.0

- Added Coilhead counterplay

## 1.1.0

- Added Turret counterplay

## 1.0.1

- Better README

## 1.0.0

- Added Jester counterplay
