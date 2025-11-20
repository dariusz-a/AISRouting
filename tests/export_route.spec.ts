import fs from 'fs';
import path from 'path';
import { test, expect, Page, TestInfo } from '@playwright/test';
import { sampleShipDataOut } from './fixtures/exportRouteFixtures';

// Helper: login using credentials specified in QA_playwright_authentication.md
async function loginAsScenarioUser(page: Page) {
  await page.goto('/');
  await page.getByTestId('username-input').fill('alice.smith@company.com');
  await page.getByTestId('password-input').fill('SecurePass123!');
  await page.getByTestId('submit-login-btn').click();
  await expect(page).toHaveURL(/.*dashboard|.*home/);
}

// Helper: create a temporary folder for exports
function makeTempFolder(testInfo: TestInfo) {
  const tmpDir = path.join(testInfo.outputDir || process.cwd(), 'export_tmp_' + Date.now());
  fs.mkdirSync(tmpDir, { recursive: true });
  return tmpDir;
}

// Helper: read and parse XML minimally
function xmlContains(xml: string, needle: string) {
  return xml.indexOf(needle) !== -1;
}

// Use existing TestData if available; otherwise tests can use fixture data to seed the app.

test.describe('Feature: Exporting Routes', () => {
  test.beforeEach(async ({ page }: { page: Page }) => {
    await loginAsScenarioUser(page);
  });

  test('Export generated track to XML creates expected file', async ({ page }: { page: Page }, testInfo: TestInfo) => {
    // Arrange: ensure application has a generated track for mmsi-1
    // This test assumes the app can be seeded via fixtures or already has test data under tests/TestData/205196000

    const tmpDir = makeTempFolder(testInfo);

    // Act: open export dialog and perform export
    await page.getByTestId('export-button').click();
    await page.getByTestId('export-output-folder').fill(tmpDir);
    await page.getByTestId('export-confirm').click();

    // Assert: expected file exists
    // Filename pattern: <mmsi>-<start>-<end>.xml - we look for any .xml in tmpDir
    const files = fs.readdirSync(tmpDir).filter((f: string) => f.endsWith('.xml'));
    expect(files.length).toBeGreaterThanOrEqual(1);

    const xml = fs.readFileSync(path.join(tmpDir, files[0]), 'utf8');
    expect(xmlContains(xml, '<RouteTemplates>')).toBeTruthy();
    expect(xmlContains(xml, '<RouteTemplate')).toBeTruthy();
    expect(xmlContains(xml, '<WayPoint')).toBeTruthy();

    // Cleanup
    fs.rmSync(tmpDir, { recursive: true, force: true });
  });

  test('Overwrite existing export file when user chooses Overwrite', async ({ page }: { page: Page }, testInfo: TestInfo) => {
    const tmpDir = makeTempFolder(testInfo);
    const filename = '205196000-20250315T000000-20250316T000000.xml';
    const target = path.join(tmpDir, filename);
    fs.writeFileSync(target, '<old/>', 'utf8');

    // Trigger export
    await page.getByTestId('export-button').click();
    await page.getByTestId('export-output-folder').fill(tmpDir);
    await page.getByTestId('export-confirm').click();

    // When conflict prompt appears, choose overwrite
    const prompt = page.getByTestId('export-conflict-prompt');
    await expect(prompt).toBeVisible();
    await prompt.getByRole('button', { name: /overwrite/i }).click();

    // Assert: file content updated and success message shown
    const xml = fs.readFileSync(target, 'utf8');
    expect(xmlContains(xml, '<RouteTemplates>')).toBeTruthy();
    await expect(page.getByTestId('export-success')).toHaveText(/export successful/i);

    fs.rmSync(tmpDir, { recursive: true, force: true });
  });

  test('Fail export when output path not writable', async ({ page }: { page: Page }) => {
    // Create a folder and make it read-only (Windows)
    const protectedDir = path.join(process.cwd(), 'protected_exports_' + Date.now());
    fs.mkdirSync(protectedDir, { recursive: true });
    try {
      // Attempt to make the folder read-only. Do not silently swallow errors.
      fs.chmodSync(protectedDir, 0o444);

      await page.getByTestId('export-button').click();
      await page.getByTestId('export-output-folder').fill(protectedDir);
      await page.getByTestId('export-confirm').click();

      await expect(page.getByTestId('export-error')).toHaveText(new RegExp(`Cannot write to output path: ${protectedDir.replace(/\\/g, '\\\\')}`));

    } finally {
        // Restore permissions and remove folder. Propagate errors rather than swallowing them.
        fs.chmodSync(protectedDir, 0o777);
      fs.rmSync(protectedDir, { recursive: true, force: true });
    }
  });

  test('Append numeric suffix on filename conflict', async ({ page }: { page: Page }, testInfo: TestInfo) => {
    const tmpDir = makeTempFolder(testInfo);
    const baseName = '205196000-20250315T000000-20250316T000000.xml';
    const target = path.join(tmpDir, baseName);
    fs.writeFileSync(target, '<old/>', 'utf8');

    await page.getByTestId('export-button').click();
    await page.getByTestId('export-output-folder').fill(tmpDir);
    await page.getByTestId('export-confirm').click();

    const prompt = page.getByTestId('export-conflict-prompt');
    await expect(prompt).toBeVisible();
    await page.getByTestId('export-conflict-suffix').click();

    // Look for new file with (1) appended
    const files = fs.readdirSync(tmpDir).filter((f: string) => f.endsWith('.xml'));
    const suffixFile = files.find((f: string) => f.includes(' (1).xml'));
    expect(suffixFile).toBeTruthy();

    await expect(page.getByTestId('export-success')).toHaveText(/export successful/i);

    fs.rmSync(tmpDir, { recursive: true, force: true });
  });

  test('WayPoint attribute mapping in generated XML', async ({ page }: { page: Page }, testInfo: TestInfo) => {
    const tmpDir = makeTempFolder(testInfo);

    await page.getByTestId('export-button').click();
    await page.getByTestId('export-output-folder').fill(tmpDir);
    await page.getByTestId('export-confirm').click();

    const files = fs.readdirSync(tmpDir).filter(f => f.endsWith('.xml'));
    expect(files.length).toBeGreaterThan(0);

    const xml = fs.readFileSync(path.join(tmpDir, files[0]), 'utf8');
    // Basic assertions for attributes
    expect(xmlContains(xml, 'Name="205196000"') || xmlContains(xml, 'Name="mmsi-1"')).toBeTruthy();
    expect(xml).toMatch(/Lat="[0-9\.-]+"/);
    expect(xml).toMatch(/Lon="[0-9\.-]+"/);
    expect(xml).toMatch(/Alt="0"/);
    expect(xml).toMatch(/TrackMode="Track"/);

    fs.rmSync(tmpDir, { recursive: true, force: true });
  });
});
