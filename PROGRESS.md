2026-10-28: Staged construction — wall 5, gate 5, watchtower 6, turret 5, spikes 3, campfire 3, hut 4; upgrades 4. Parts are separate greybox pieces with a shared StagedBuild.
2026-10-28: Upright trees, baked grass/dirt ground, and a mobile quality budget. Clip choppiness on this VM is llvmpipe, about 110 ms a frame after the pass.
2026-10-28: Treeline trunks stand up. Stand() had wiped the Blender Z-up node, and the old bounds test skipped CommonTree because the canopy is wider than the trunk. Pad labels are offset and fade with distance.
