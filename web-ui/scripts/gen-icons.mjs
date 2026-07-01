// Render brand/logo-master.svg -> transparent PNG icons in public/.
// resvg preserves alpha (Quick Look/qlmanage flattens onto white — don't use it).
//   node scripts/gen-icons.mjs
import { readFileSync, writeFileSync } from 'node:fs'
import { Resvg } from '@resvg/resvg-js'

const svg = readFileSync(new URL('../brand/logo-master.svg', import.meta.url))

// One transparent PNG serves tab favicon + apple-touch + in-app logo (browsers downscale).
const size = 512
const resvg = new Resvg(svg, {
  fitTo: { mode: 'width', value: size },
  // no `background` => fully transparent outside the squircle
})
writeFileSync(new URL('../public/logo.png', import.meta.url), resvg.render().asPng())
console.log(`  logo.png  ${size}x${size}`)
