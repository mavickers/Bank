import { defineConfig } from 'vite'

export default defineConfig({
    build: {
        lib: {
            entry: './src/main.jsx',
            name: 'ReactViteTest',
            fileName: 'react-vite-test'
        },
        manifest: true,
        minify: false,
        rollupOptions: {
            external: ['react', 'react-dom'],
            output: {
                globals: {
                    'react': 'React',
                    'react-dom': 'ReactDOM'
                }
            }
        },
        target: 'modules'
    },
    define: {
        'process.env': { NODE_ENV: 'production' }
    }
})
