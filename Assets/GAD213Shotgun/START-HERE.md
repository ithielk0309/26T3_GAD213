# GAD213 — four-shell interaction range
Unity 6.0.72f1. Requires Unity UI (uGUI) and either the legacy Input Manager or the Input System. No external models, animation clips or sound files are required.

1. Open your GAD213 project. If this folder isn't already under Assets, import the supplied unitypackage.
2. Outside Play mode, choose **Tools → GAD213 → Open or Create Shotgun Range**. The first use saves **Assets/GAD213Shotgun/Range/Shotgun Range.unity**, four shell assets, materials, a fire-patch prefab and a practice-enemy prefab. Later uses open the saved scene. Existing scenes/build settings are not replaced.
3. In the Hierarchy expand **Practice player → View (add your own shotgun here)**. Place your own shotgun prefab under View. Create a child transform called **Muzzle** at the barrel tip. On Practice player's **Weapon System**, drag the prefab instance into **Shotgun View** and its child into **Muzzle**. With both empty you can test from the camera; no shotgun model or prefab is supplied.
4. Save. Press Play. WASD/mouse move and look. Hold Q, move toward a slot, release Q to choose. Release near the centre to keep your previous choice. Hold R to insert; tap R to pump; left click to fire. Esc releases the mouse; click to resume. T resets enemy health and positions. Restart Play to reset ammunition.

**First mixed load:** load buckshot, pump it into the chamber, then choose Beanbag and insert one. The HUD still shows buckshot in the chamber. Tap R: the live buckshot is discarded, and Beanbag enters the chamber. Fire, then pump again. Ejecting a spent casing should not increase the live-discard count.

**Four shell assets**, ordered clockwise from the top: Buckshot (9 spread pellets, capped total push), Slug (one accurate heavy hit), Beanbag (low damage, push and 1.5 seconds of stun), Dragon's Breath (short-range direct burn or a 3-second fire patch on an Ignitable Floor). Burning refreshes without stacking. Resize the square wheel uniformly in the Canvas. Assign optional firing clips on each shell and insert/pump clips on the controller.

**Enemy physics and animation:** The root owns the capsule and Rigidbody. Waypoint movement uses the kinematic body; a hit temporarily switches to physics and disables movement/attacks during recovery. Assign a child Animator for your own clips (optional triggers: Hit, Stun, Attack; root motion off). The sample uses a visible lean, stagger and lunge without clips. Dead targets deactivate; T restores them. Enemy contact invokes On Attack for your own player health; the practice scene doesn't damage the player.

**Fire routing:** Place fire across the direct approach near the divider. The cautious enemy chooses an open route through the authored corners or waits if blocked; the tank ignores fire when choosing its route but still burns. Move corner markers if you reshape the walls. Mark new solid obstacles with Range Wall and burnable floors with Ignitable Floor.

**Using your own movement scene:** Keep your own player controller. Assign its camera, optional model and muzzle to Weapon System, copy the range Canvas and reassign both HUD links, plus the four shell assets and fire-patch prefab. Range Player is the optional demo movement script; don't run two look controllers. Gate your own mouse-look while Q is held. Add an encounter's target and route references on each enemy instance.
