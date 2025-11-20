import { defineConfig } from '@playwright/test';

export default defineConfig({
  testDir: './tests',
  retries: 0,
  timeout: 30_000,
  use: {
    headless: true,
    baseURL: 'http://localhost:5173'
  }
});
