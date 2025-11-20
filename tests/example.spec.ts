import { test, expect } from '@playwright/test';

test.describe('AISRouting example tests', () => {
  test('login form controls are visible and working (getByTestId)', async ({ page }) => {
    await page.goto('/');

    const loginForm = page.getByTestId('login-form');
    await expect(loginForm).toBeVisible();

    await loginForm.getByTestId('username-input').fill('ada');
    await loginForm.getByTestId('password-input').fill('correcthorsebatterystaple');
    await expect(loginForm.getByTestId('username-input')).toHaveValue('ada');

    await loginForm.getByTestId('submit-login-btn').click();
    // form has no real submit handler in this static example — assert the button exists
    await expect(page.getByTestId('submit-login-btn')).toBeVisible();
  });

  test('product list and add button exist', async ({ page }) => {
    await page.goto('/');

    const list = page.getByTestId('product-list');
    await expect(list).toBeVisible();

    const card = list.getByTestId('product-card-42');
    await expect(card).toBeVisible();

    await card.getByTestId('add-to-cart-42').click();
    await expect(card.getByTestId('add-to-cart-42')).toBeVisible();
  });
});
