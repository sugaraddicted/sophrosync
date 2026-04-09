```markdown
# Design System Specification: The Modern Archivist

## 1. Overview & Creative North Star
**Creative North Star: "The Clinical Editorial"**
This design system rejects the over-rounded, neon-saturated aesthetic of modern SaaS in favor of a look that is structured, intellectual, and profoundly calm. It is inspired by high-end botanical journals and mid-century architectural archives. By combining the authoritative weight of editorial serif typography with a "Soft Power" palette of sage and cream, we create an environment of "Clinical Excellence."

The system moves beyond the "template" look by utilizing **intentional asymmetry** (e.g., large left-aligned display type balanced by significant negative space on the right) and **tonal depth** rather than structural lines. We treat the screen not as a digital interface, but as a series of premium tactile layers.

---

## 2. Colors & Surface Architecture
The palette is rooted in nature and stability. We utilize Material Design naming conventions but apply them with a "Soft Power" philosophy.

### The Palette
- **Background (`#fafaf5`)**: Our canvas. A warm, breathable cream that avoids the sterile "hospital white."
- **Primary (`#546253`)**: A mid-tone Sage Green. It represents growth and stability. Use this for key actions and authoritative accents.
- **Secondary (`#5f5f5f`)**: A Warm Grey. Used to ground the interface in professional "Archivist" tones.
- **Surface Tiers**: Use `surface-container-low` (`#f3f4ee`) through `highest` (`#dee4da`) to create structural hierarchy.

### The "No-Line" Rule
**Explicit Instruction:** Do not use 1px solid borders to section content. Boundaries must be defined solely through background color shifts.
- To separate a sidebar, use `surface-container-low` against the `background`.
- To highlight a content area, use a `surface-container` shift.
- Standard 100% opaque borders are strictly prohibited as they create "visual noise" that contradicts the "Soft Power" calm.

### Signature Textures & Gradients
To avoid a flat, "out-of-the-box" feel:
- **Subtle Soul:** For hero sections or primary CTAs, use a subtle linear gradient transitioning from `primary` (`#546253`) to `primary-dim` (`#485648`) at a 145-degree angle.
- **Glassmorphism:** For floating navigation or modal overlays, use `surface-bright` at 85% opacity with a `backdrop-blur` of 12px. This allows the sage and cream tones to bleed through, softening the interface.

---

## 3. Typography
The typography is the "Voice" of the therapist: authoritative yet empathetic.

- **Display & Headlines (Newsreader):** Use for all editorial moments. The high x-height and elegant serifs convey "Clinical Excellence."
    - *Style Note:* Use `display-lg` (3.5rem) with tighter letter-spacing (-0.02em) for a high-fashion, editorial impact.
- **Body & Labels (Work Sans):** A clean, architectural sans-serif. It provides the "Modern" in "Modern Archivist."
    - *Style Note:* For `body-md`, ensure a line-height of at least 1.6 to maintain a feeling of "breathing room."
- **Hierarchy:** Use `title-lg` (Work Sans, Medium weight) for functional headers and `headline-md` (Newsreader) for narrative headers. Never mix the two within the same visual grouping.

---

## 4. Elevation & Depth
We convey hierarchy through **Tonal Layering** rather than traditional drop shadows.

- **The Layering Principle:** Stacking is the primary method of organization.
    - *Example:* Place a `surface-container-lowest` (#ffffff) card on top of a `surface-container-low` (#f3f4ee) section. The slight shift in "warmth" creates a natural lift.
- **Ambient Shadows:** When a floating element (like a modal) is required, use an extra-diffused shadow: `box-shadow: 0 20px 40px rgba(46, 52, 45, 0.06)`. The shadow color is derived from `on-surface`, creating a natural ambient occlusion rather than a "dirty" grey shadow.
- **The "Ghost Border" Fallback:** If a container requires a boundary for accessibility, use the `outline-variant` (`#aeb4aa`) at **15% opacity**. It should be felt, not seen.

---

## 5. Components
All components follow a **reduced roundness scale** (`DEFAULT: 0.25rem`) to maintain a professional, structured feel.

- **Buttons:**
    - *Primary:* `primary` background with `on-primary` text. No border. `0.25rem` radius.
    - *Secondary:* `secondary-container` background.
    - *Tertiary:* Ghost style. Text only in `primary` weight, moving to a subtle `surface-container-high` background on hover.
- **Input Fields:**
    - Use `surface-container-low` as the fill.
    - Bottom-border only (2px) using `outline-variant` for a "stationery" look, or a full `0.25rem` radius with a "Ghost Border."
- **Cards & Lists:**
    - **Prohibition:** Do not use divider lines between list items.
    - **Execution:** Use vertical spacing (Scale `4` or `1.4rem`) to separate items. For lists, use a alternating subtle background shift (`surface` to `surface-container-low`) for high-density data.
- **Status Chips:**
    - Use `tertiary-container` (#ffe9e9) for soft alerts. Avoid "Emergency Red" unless critical; use `error` (#9e422c) sparingly to maintain the calm.
- **The "Archivist" Timeline:**
    - A custom component for therapy notes. Use a single vertical 2px line in `outline-variant` (20% opacity) with `primary` dots to indicate clinical milestones.

---

## 6. Do’s and Don'ts

### Do:
- **Embrace White Space:** Use the `24` (8.5rem) spacing token for top/bottom margins on major sections to create an "Editorial" feel.
- **Align to a Grid, but Break it:** Place images or pull-quotes slightly off-center or overlapping container edges to create a "curated" look.
- **Use Mid-tones:** Lean heavily on the Sage (`primary`) and Warm Grey (`secondary`) to avoid the interface feeling too "bright/cheap" or too "dark/heavy."

### Don't:
- **No "Pill" Buttons:** Never use `full` (9999px) roundness for buttons. It is too "Tech-Startup" and undermines the "Clinical Excellence" authority.
- **No Pure Black:** Never use `#000000`. Use `on-surface` (#2e342d) for all "black" text to maintain tonal harmony with the green palette.
- **No Heavy Borders:** If you feel the need to add a border, try adding a background color shift or more padding first. 1px solid lines are a last resort.