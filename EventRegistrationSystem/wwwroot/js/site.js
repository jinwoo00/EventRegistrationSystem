// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
@import "@tailwindcss"
<script>
    tailwind.config = {
        theme: {
        extend: {
        animation: {
        'pulse-slow': 'pulse 3s cubic-bezier(0.4, 0, 0.6, 1) infinite',
                    },
    backgroundImage: {
        'gradient-bg': 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
                    },
    boxShadow: {
        'soft': '0 10px 40px rgba(0, 0, 0, 0.08)',
    'xl-soft': '0 20px 60px rgba(0, 0, 0, 0.12)'
                    }
                }
            }
        }
</script>