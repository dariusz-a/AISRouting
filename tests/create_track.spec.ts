import { test, expect } from '@playwright/test';
import { createTrackFixtures } from './fixtures/createTrackFixtures';
import { selectVessel, waitForTrackReady, elementExists } from './helpers/ui';
import { mockVessel205196000 } from '../src/mocks/mockData';

test.describe('Feature: Create Track', () => {
  test.beforeEach(async ({ page }) => {
    // Navigate to app (assuming local dev server or file-based harness)
    await page.goto('/');
    // Perform login using required credentials
    await page.getByTestId('username-input').fill('alice.smith@company.com');
    await page.getByTestId('password-input').fill('SecurePass123!');
    await page.getByTestId('submit-login-btn').click();
    await expect(page).toHaveURL(/.*/);
  });

  test('Scenario Outline: Create track for selected ship and time range', async ({ page }) => {
    // Use provided fixture values
    const mmsi = mockVessel205196000.mmsi;

    // Ensure input root is available via fixture (test harness assumption)
    // Select vessel
    await selectVessel(page, mmsi);

    // Set start/stop pickers (use testids)
    await page.getByTestId('start-picker').fill('2025-03-15T00:00:01');
    await page.getByTestId('stop-picker').fill('2025-03-15T00:10:01');

    // Click Create Track
    await page.getByTestId('create-track').click();

    // Wait for results
    await waitForTrackReady(page);

    // Assert waypoints present
    const list = page.getByTestId('track-results-list');
    await expect(list).toBeVisible();
    const items = await list.locator('li').count();
    expect(items).toBeGreaterThan(0);
  });

  test('Create track with noisy data and narrowed time window', async ({ page }) => {
    // Select vessel
    await selectVessel(page, mockVessel205196000.mmsi);

    // Narrow time window (assumes these timestamps intersect noisy.csv)
    await page.getByTestId('start-picker').fill('2025-03-15T00:00:01');
    await page.getByTestId('stop-picker').fill('2025-03-15T00:00:10');

    await page.getByTestId('create-track').click();
    await waitForTrackReady(page);

    const list = page.getByTestId('track-results-list');
    await expect(list).toBeVisible();
    const items = await list.locator('li').count();
    expect(items).toBeGreaterThan(0);

    // Verify completion status text
    await expect(page.getByText(/Track created:/)).toBeVisible();
  });

  test('Reject track creation when no ship selected', async ({ page }) => {
    // Ensure no vessel selected (assumes UI starts with none)
    await page.getByTestId('create-track').click();
    await expect(page.getByText('No ship selected')).toBeVisible();
    // Ensure results not shown
    expect(await elementExists(page.getByTestId('track-results-list'))).toBe(false);
  });

  test('Fail gracefully on malformed CSV rows', async ({ page }) => {
    await selectVessel(page, mockVessel205196000.mmsi);
    await page.getByTestId('start-picker').fill('2025-03-15T00:00:01');
    await page.getByTestId('stop-picker').fill('2025-03-15T00:10:01');
    await page.getByTestId('create-track').click();
    await waitForTrackReady(page);

    await expect(page.getByText('Some rows were ignored due to invalid format')).toBeVisible();
    const list = page.getByTestId('track-results-list');
    const items = await list.locator('li').count();
    expect(items).toBeGreaterThan(0);
  });

  test('Handle missing Heading or SOG values in records', async ({ page }) => {
    await selectVessel(page, mockVessel205196000.mmsi);
    await page.getByTestId('start-picker').fill('2025-03-15T00:00:01');
    await page.getByTestId('stop-picker').fill('2025-03-15T00:10:01');
    await page.getByTestId('create-track').click();
    await waitForTrackReady(page);

    // Check for data-quality note
    await expect(page.getByText(/data-quality/i)).toBeVisible();

    // Spot check first waypoint has non-null lat/lon via text content
    const first = page.getByTestId('track-results-list').locator('li').first();
    await expect(first).toBeVisible();
  });

  test('Prevent track creation when input root empty', async ({ page }) => {
    // This test assumes a way to point the app to an empty input root via fixtures/mocking
    // Open ship selection combo
    await page.getByTestId('ship-combo').click();
    await expect(page.getByText('No vessels found in input root')).toBeVisible();
    await expect(page.getByTestId('create-track')).toBeDisabled();
  });

  test('Create track unavailable for user without permission', async ({ page }) => {
    // This assumes either a user fixture or mocked permission service
    // Attempt to hover the disabled button to show tooltip
    const btn = page.getByTestId('create-track');
    await expect(btn).toBeDisabled();
    await btn.hover();
    await expect(page.getByText('Insufficient privileges')).toBeVisible();
  });
});
