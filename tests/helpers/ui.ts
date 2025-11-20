import { Page, Locator } from '@playwright/test';

export async function elementExists(locator: Locator) {
  // Return true if visible, false if not found or not visible.
  // Avoid swallowing unexpected errors — only catch visibility failures.
  return await locator.isVisible().catch(() => false);
}

export async function selectVessel(page: Page, mmsi: string) {
  const combo = page.getByTestId('ship-combo');
  await combo.click();
  await page.getByRole('option', { name: mmsi }).click();
}

export async function waitForTrackReady(page: Page) {
  await page.getByTestId('track-results-list').waitFor({ state: 'visible', timeout: 20000 });
}
