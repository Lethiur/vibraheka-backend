export default [
  {
    files: ['**/*.ts', '**/*.tsx'],

    // ❌ NO pongas `import parser from '@typescript-eslint/parser'`
    // ✅ Usa string con el nombre del parser
    languageOptions: {
      parser: '@typescript-eslint/parser',
      parserOptions: {
        ecmaVersion: 2020,
        sourceType: 'module'
      }
    },

    plugins: {
      '@typescript-eslint': '@typescript-eslint',
      import: 'import',
      prettier: 'prettier'
    },

    rules: {
      // Prettier
      'prettier/prettier': 'error',

      // Orden de imports
      'import/order': [
        'error',
        {
          groups: ['builtin', 'external', 'internal', ['parent', 'sibling', 'index']],
          pathGroups: [
            {
              pattern: '@/**',
              group: 'internal'
            }
          ],
          pathGroupsExcludedImportTypes: ['builtin'],
          alphabetize: { order: 'asc', caseInsensitive: true },
          'newlines-between': 'always'
        }
      ],

      'import/no-unresolved': 'error',
      'import/no-duplicates': 'error',
      'import/no-named-as-default': 'off'
    },

    settings: {
      'import/resolver': {
        typescript: {}
      }
    }
  }
];
