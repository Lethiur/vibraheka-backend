// jest.config.ts
import { pathsToModuleNameMapper } from 'ts-jest'
import { fileURLToPath } from 'url'
import path from 'path'
import fs from 'fs'

const __filename = fileURLToPath(import.meta.url)
const __dirname = path.dirname(__filename)
const tsconfig = JSON.parse(
    fs.readFileSync(path.resolve(__dirname, './tsconfig.json'), 'utf-8')
);

export default {
    preset: 'ts-jest/presets/default-esm',
    testEnvironment: 'node',
    // CAMBIO AQUÍ: Eliminamos el '/' después de <rootDir> 
    // y nos aseguramos de que apunte a donde está el código real.
    moduleNameMapper: {
        // 1. Mantenemos tus alias (@Data, @Domain, etc.)
        ...pathsToModuleNameMapper(tsconfig.compilerOptions.paths, {
            prefix: '<rootDir>',
        }),

        // 2. AÑADIMOS EL FIX: Forzamos a Jest a usar la misma instancia de memoria para el SDK
        // Esto soluciona el problema de que 'instanceof' devuelva false
        '^@aws-sdk/client-dynamodb$': '<rootDir>/node_modules/@aws-sdk/client-dynamodb',
    },
    transform: {
        '^.+\\.tsx?$': [
            'ts-jest',
            {
                useESM: true,
                tsconfig: 'tsconfig.json' 
            },
        ],
    },
    transformIgnorePatterns: [
        'node_modules/(?!(@aws-sdk)/)'
    ],
    extensionsToTreatAsEsm: ['.ts'],
    // Añade esto para que Jest sepa dónde buscar archivos
    modulePaths: [tsconfig.compilerOptions.baseUrl],
};