# 26T3 GAD213 - Shotgun system

Open Assets/Scenes/SampleScene.unity after Unity compiles. If it was already open, reopen the saved scene from the Project window.

The existing Player movement, crouching and stamina are retained. The player and camera are set to normal scale for the encounter room. Camera reference and ground detection are connected. The original Ground is preserved but disabled beneath the new room.

Space: start random encounter; 8 seconds to prepare. Q + mouse then release: select shell. Hold R: insert shells. Tap R: pump (discards a live chambered shell). Left click: shoot. T: clear encounter. Escape: release mouse.

On Player > Weapon System, edit reload/pump clips and volumes, insertion timing, and firing audio per shell. Shell assets under Range contain damage, spread, stun and fire settings. Beanbag.asset is labelled Pepperbag in game.

Edit ShotgunModel/Pump Shotgun.prefab for the weapon model. Enemy prefabs under DemoEnemies expose burning sound/volume and reaction settings. Range/Fire patch.prefab contains editable flames/smoke and burning audio. Select Random encounters - Space to start to edit count, preparation time, spawn points and random seed. Ammo reserves refill between rounds by default.

The old standalone range remains available. The source Chaplin Avenue and Shotgun Prototype 1 projects were not changed by this transfer.

Scripts compile and scene references/audio dependencies were checked. Unity Play-mode behaviour still needs verification.
