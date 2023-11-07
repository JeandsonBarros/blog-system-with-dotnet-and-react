/** @type {import('tailwindcss').Config} */
// eslint-disable-next-line @typescript-eslint/no-var-requires, no-undef
const { nextui } = require("@nextui-org/react");

export default {
  content: [
    "./index.html",
    "./src/**/*.{js,ts,jsx,tsx}",
    "./node_modules/@nextui-org/theme/dist/**/*.{js,ts,jsx,tsx}"
  ],
  theme: {
    extend: {},
  },
  darkMode: "class",
  plugins: [nextui({
    themes: {
      light: {
        colors: {
          background: "#FFFFFF", // or DEFAULT
          foreground: "#11181C", // or 50 to 900 DEFAULT
          primary: {
            //... 50 to 900
            foreground: "#FFFFFF",
            DEFAULT: "#026773",
          },
          secondary:{
            //... 50 to 900
            foreground: "#11181C",
            DEFAULT: "#F2E3D5",
          }
          // ... rest of the colors
        },
      },
      dark: {
        colors: {
          background: "#000000", // or DEFAULT
          foreground: "#ECEDEE", // or 50 to 900 DEFAULT
          primary: {
            //... 50 to 900
            foreground: "#FFFFFF",
            DEFAULT: "#0267",
          },
        },
        // ... rest of the colors
      }
    },
  })],
}