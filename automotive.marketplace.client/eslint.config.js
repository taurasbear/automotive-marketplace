import js from "@eslint/js";
import importPlugin from "eslint-plugin-import";
import react from "eslint-plugin-react";
import reactHooks from "eslint-plugin-react-hooks";
import reactRefresh from "eslint-plugin-react-refresh";
import globals from "globals";
import tseslint from "typescript-eslint";

export default tseslint.config(
  { ignores: ["dist", "src/components/ui/**"] },
  {
    extends: [
      js.configs.recommended,
      ...tseslint.configs.recommendedTypeChecked,
    ],
    files: ["**/*.{ts,tsx}"],
    settings: {
      react: { version: "detect" },
    },
    languageOptions: {
      ecmaVersion: 2020,
      globals: globals.browser,
      parserOptions: {
        project: ["./tsconfig.node.json", "./tsconfig.app.json"],
        tsconfigRootDir: import.meta.dirname,
      },
    },
    plugins: {
      "react-hooks": reactHooks,
      "react-refresh": reactRefresh,
      react: react,
      import: importPlugin,
    },
    rules: {
      ...reactHooks.configs.recommended.rules,
      ...react.configs.recommended.rules,
      ...react.configs["jsx-runtime"].rules,
      "import/no-cycle": "error",
      "react-refresh/only-export-components": [
        "warn",
        { allowConstantExport: true },
      ],
      "@typescript-eslint/no-misused-promises": [
        "error",
        {
          checksVoidReturn: { attributes: true },
        },
      ],
      "no-restricted-imports": [
        "error",
        {
          patterns: [
            {
              group: [
                "@/features/*/api/*",
                "@/features/*/components/*",
                "@/features/*/types/*",
                "@/features/*/state/*",
                "@/features/*/schemas/*",
                "**/features/*/api/*",
                "**/features/*/components/*",
                "**/features/*/types/*",
                "**/features/*/state/*",
                "**/features/*/schemas/*",
              ],
              message:
                "Import from feature barrel files (index.ts) instead of internal modules.",
            },
            {
              group: ["@/features/*/!(index)", "**/features/*/!(index)"],
              message:
                "Import from feature barrel files (index.ts) instead of internal modules.",
            },
          ],
        },
      ],
    },
  },
);
