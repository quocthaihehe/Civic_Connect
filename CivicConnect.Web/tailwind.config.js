/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    './Pages/**/*.cshtml',
    './Views/**/*.cshtml',
    './Areas/**/*.cshtml',
    './wwwroot/**/*.html',
    './wwwroot/**/*.js'
  ],
  theme: {
    extend: {
      colors: {
        'primary': '#133B2F',
        'success': '#22C55E',
        'warning': '#E6C200',
        'danger': '#EF4444',
        'background': '#FDFBF7',
        'text-main': '#2C3531'
      },
      fontFamily: {
        'sans': ['Inter', 'sans-serif'],
        'serif': ['"EB Garamond"', 'serif']
      }
    },
  },
  plugins: [],
}
