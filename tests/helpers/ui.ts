import { Page, Locator } from '@playwright/test';

export async function elementExists(locator: Locator) {
  try { return await locator.isVisible(); } catch { return false; }
}

export async function selectVessel(page: Page, mmsi: string) {
  const combo = page.getByTestId('ship-combo');
  await combo.click();
  await page.getByRole('option', { name: mmsi }).click();
}

export async function waitForTrackReady(page: Page) {
  await page.getByTestId('track-results-list').waitFor({ state: 'visible', timeout: 20000 });
}
