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
        'primary': '#2563EB',
        'success': '#22C55E',
        'warning': '#F59E0B',
        'danger': '#EF4444',
        'background': '#F8FAFC',
      }
    },
  },
  plugins: [],
}
