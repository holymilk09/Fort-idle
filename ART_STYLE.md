# Fort Idle art style

Bright, chunky low-poly, the same family as the original wood-stack ad. Shapes read as silhouettes from the top-down camera: a short wall of planks, a taller stone wall, a helmet, a shambler, a pine with eyes. Surfaces stay flat and matte. One warm sun, soft shadows, and a cool grade after dusk do the lighting. No trim sheets, no realistic bark, no paid plugins.

## Palette

Hex values are the daytime read. Mesh packs bring their own flat colors; these are the anchors the scene is graded toward.

| Role | Hex | Use |
| --- | --- | --- |
| Day sky | `#94C2EB` | Camera background |
| Day sun | `#FFF5DB` | One directional light, warm |
| Canopy | `#2E7533` | Tree crowns, forest edge |
| Grass | `#577542` | Ground |
| Dirt | `#735C38` | Clearing around the fire |
| Wood | `#614021` | Trunks, logs, wood wall |
| Reinforced | `#7A7061` | Stone courses with a wood top |
| Stone | `#A8ADB3` | Stone wall, cooler and taller |
| Skin | `#EDC294` | Hero and workers |
| Fire | `#FF7A14` | Flame and the camp point light |
| Ember | `#4D2914` | Logs under the fire |
| Night sky | `#0D1224` | Dusk into night |
| Night sun | `#8CA8F2` | Cool blue fill |
| Night grade | `#6E7BB8` | Blue-purple color filter after dusk |
| Shambler | `#3A4A32` | Dull green-grey zombie |
| Brute | `#6B2A1F` | Dark armor, reads heavier than a shambler |
| Iron | `#2E3848` | Small dark metal chunk, distinct from stone |
| Eye | `#FF381F` | Treant eyes only |

## Per element

- **Ground.** Warm grass plane. The forest edge is tufts, bushes, and flowers, not a texture splat.
- **Clearing.** Flat dirt disc under the camp so the fort reads as a worked patch.
- **Trees.** Chunky trunks and blob crowns. Three shapes (common, second common, birch) so a row is not a stamp. Stumps stay low and round.
- **Rocks.** Mossy and grey boulders, separate from the iron drop.
- **Hero.** Viking silhouette, helmet and shoulders readable from above. Carries the log stack, not a grey capsule.
- **Worker.** Smaller hard-hat worker. Same stack language as the hero.
- **Shambler.** Zombie, dull and narrow. No floating primitive eyes.
- **Brute.** Armored knight, scaled bulkier than a shambler. Goes for walls first; the mass should say so before the health bar does.
- **Treant.** No CC0 treant in the packs we could use. A pine, scaled up and tinted a darker yellow-green, with two ember eyes. Closest match, called out as such.
- **Wood stack.** Short round logs, the ad shape, not flat boxes.
- **Stone pickup.** Mossy pebble. **Iron pickup.** Smaller, darker, metallic, a different rock mesh. They must not both read as grey cubes.
- **Wood wall.** Vertical timber palisade, warm brown, the lowest tier.
- **Reinforced wall.** Grey stone courses with a wood battlement. Taller than wood.
- **Stone wall.** Solid stone blocks, cooler grey, tallest tier. No wood cap, so it does not match the reinforced wall from above.
- **Gate.** Timber gate with a stone frame, in the lane south of the wood wall.
- **Watchtower.** Square stone tower with a worker on the roof. Taller than the turret.
- **Turret.** Siege ballista. It yaws to shoot. The upgrade does not change the mesh.
- **Spikes.** A low row of sharpened stakes. Shamblers only.
- **Fire.** Log campfire plus an orange flame and one short-range point light. No shadows from that light.
- **Night.** The sun goes blue, ambient drops, and a color grade pushes blue-purple. Fire stays orange so the camp is the warm spot.

## Camera and performance

The camera sits high and looks down. Prefer a strong top silhouette over detail on the underside. One directional soft shadow, one extra light on the fire, shared materials, no extra render features, no per-plank material instances. Gameplay numbers stay in `FortIdleConfig`.
