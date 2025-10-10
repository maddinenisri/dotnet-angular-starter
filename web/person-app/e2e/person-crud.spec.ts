import { test, expect } from '@playwright/test';

test.describe('Person Management - End-to-End Tests', () => {

  test.beforeEach(async ({ page }) => {
    await page.goto('/');
    await page.waitForLoadState('networkidle');
  });

  test('should display the person list page', async ({ page }) => {
    await expect(page).toHaveTitle(/PersonApp/);
    await expect(page.locator('h2').first()).toContainText('Person Management');
    await expect(page.locator('a:has-text("Add New Person")')).toBeVisible();
  });

  test('should create a new person', async ({ page }) => {
    await page.click('a:has-text("Add New Person")');
    await expect(page.locator('h3:has-text("Create New Person")')).toBeVisible();

    await page.fill('input[formControlName="name"]', 'John Doe');
    await page.fill('input[formControlName="age"]', '30');
    await page.fill('input[formControlName="dateOfBirth"]', '1994-01-15');

    await page.fill('input[placeholder="Add a skill..."]', 'JavaScript');
    await page.click('button:has-text("Add")');
    await page.fill('input[placeholder="Add a skill..."]', 'TypeScript');
    await page.click('button:has-text("Add")');
    await page.fill('input[placeholder="Add a skill..."]', 'Angular');
    await page.click('button:has-text("Add")');

    await page.click('button[type="submit"]:has-text("Create Person")');
    await page.waitForURL(/.*\/persons$/);

    await expect(page.locator('text=John Doe')).toBeVisible();
    await expect(page.locator('text=JavaScript')).toBeVisible();
  });

  test('should edit an existing person', async ({ page }) => {
    const originalName = 'Edit Test ' + Date.now();
    const updatedName = 'Updated ' + Date.now();

    // Create person
    await page.click('a:has-text("Add New Person")');
    await page.fill('input[formControlName="name"]', originalName);
    await page.fill('input[formControlName="age"]', '28');
    await page.fill('input[formControlName="dateOfBirth"]', '1996-03-10');
    await page.fill('input[placeholder="Add a skill..."]', 'Editing');
    await page.click('button:has-text("Add")');
    await page.click('button[type="submit"]:has-text("Create Person")');
    await page.waitForURL(/.*\/persons$/);
    await page.waitForTimeout(1000);

    // Edit person
    const row = page.locator('tr', { hasText: originalName }).first();
    await row.locator('a.btn-warning').click();
    await expect(page.locator('h3:has-text("Edit Person")')).toBeVisible();

    await page.fill('input[formControlName="name"]', updatedName);
    await page.fill('input[formControlName="age"]', '29');
    await page.click('button[type="submit"]:has-text("Update Person")');
    await page.waitForURL(/.*\/persons$/);
    await page.waitForTimeout(1000);

    await expect(page.locator(`text=${updatedName}`)).toBeVisible();
  });

  test('should delete a person', async ({ page }) => {
    const personToDelete = 'Delete Test ' + Date.now();

    // Create person
    await page.click('a:has-text("Add New Person")');
    await page.fill('input[formControlName="name"]', personToDelete);
    await page.fill('input[formControlName="age"]', '35');
    await page.fill('input[formControlName="dateOfBirth"]', '1989-08-25');
    await page.fill('input[placeholder="Add a skill..."]', 'Temporary');
    await page.click('button:has-text("Add")');
    await page.click('button[type="submit"]:has-text("Create Person")');
    await page.waitForURL(/.*\/persons$/);
    await page.waitForTimeout(1000);

    await expect(page.locator(`text=${personToDelete}`)).toBeVisible();

    // Delete person
    page.on('dialog', dialog => dialog.accept());
    const row = page.locator('tr', { hasText: personToDelete }).first();
    await row.locator('button.btn-danger:has-text("Delete")').click();
    await page.waitForTimeout(1000);

    await expect(page.locator(`text=${personToDelete}`)).not.toBeVisible();
  });

  test('should validate required fields', async ({ page }) => {
    await page.click('a:has-text("Add New Person")');
    await page.click('button[type="submit"]:has-text("Create Person")');

    await expect(page.locator('text=Name is required')).toBeVisible();
    await expect(page.locator('text=Age is required')).toBeVisible();
    await expect(page.locator('text=Date of birth is required')).toBeVisible();
  });

  test('should validate age range', async ({ page }) => {
    await page.click('a:has-text("Add New Person")');

    await page.fill('input[formControlName="name"]', 'Test Person');
    await page.fill('input[formControlName="age"]', '-5');
    await page.fill('input[formControlName="dateOfBirth"]', '2000-01-01');
    await page.click('button[type="submit"]:has-text("Create Person")');

    await expect(page.locator('text=Age must be between 0 and 150')).toBeVisible();
  });

  test('should cancel form and return to list', async ({ page }) => {
    await page.click('a:has-text("Add New Person")');
    await page.fill('input[formControlName="name"]', 'Temporary');
    await page.fill('input[formControlName="age"]', '25');
    await page.click('button:has-text("Cancel")');

    await expect(page).toHaveURL(/.*\/persons$/);
    await expect(page.locator('h2').first()).toContainText('Person Management');
  });

  test('should remove a skill from the form', async ({ page }) => {
    await page.click('a:has-text("Add New Person")');

    // Add skills
    await page.fill('input[placeholder="Add a skill..."]', 'Skill 1');
    await page.click('button:has-text("Add")');
    await page.fill('input[placeholder="Add a skill..."]', 'Skill 2');
    await page.click('button:has-text("Add")');
    await page.fill('input[placeholder="Add a skill..."]', 'Skill 3');
    await page.click('button:has-text("Add")');

    await expect(page.locator('text=Skill 1')).toBeVisible();
    await expect(page.locator('text=Skill 2')).toBeVisible();
    await expect(page.locator('text=Skill 3')).toBeVisible();

    // Remove the second skill
    const badges = page.locator('.badge.bg-info');
    const count = await badges.count();
    expect(count).toBe(3);

    // Click the remove button for the second skill
    await page.locator('button.btn-sm.btn-danger').nth(1).click();

    // Verify skill was removed
    const remainingCount = await page.locator('.badge.bg-info').count();
    expect(remainingCount).toBe(2);
  });
});
