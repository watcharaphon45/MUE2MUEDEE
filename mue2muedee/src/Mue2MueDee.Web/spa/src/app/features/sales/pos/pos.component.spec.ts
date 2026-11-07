import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideRouter } from '@angular/router';
import { PosComponent } from './pos.component';
import { InventoryItem } from '../../inventory/inventory.models';

describe('POS paginated cart actions', () => {
  async function createCart() {
    TestBed.configureTestingModule({ providers: [provideHttpClient(), provideHttpClientTesting(), provideRouter([])] });
    const fixture = TestBed.createComponent(PosComponent);
    fixture.componentInstance.cartItems.set(Array.from({ length: 32 }, (_, index) => ({
      inventoryItem: {
        id: `item-${index}`, sku: `SKU-${index}`, serialNumber: `SN-${index}`,
        brandName: 'Sony', productModelName: `MODEL-${index}`, actualCost: 750,
        conditionGradeCode: 'A'
      } as InventoryItem,
      unitPrice: 1000, discount: 0, subTotal: 1000, grossProfit: 250, marginPercentage: 25, warrantyDays: 30
    })));
    fixture.detectChanges();
    TestBed.inject(HttpTestingController).expectOne('/api/sticker-templates').flush({ data: [] });
    await fixture.whenStable();
    const next = fixture.nativeElement.querySelector('app-table-pagination [aria-label="หน้าถัดไป"]') as HTMLButtonElement;
    next.click();
    fixture.detectChanges();
    return fixture;
  }

  it('edits and removes the correct desktop row on page two while retaining full-cart totals', async () => {
    const fixture = await createCart();
    const rows = fixture.nativeElement.querySelectorAll('.cart-table tbody tr') as NodeListOf<HTMLElement>;
    expect(rows.length).toBe(15);
    expect(rows[0].textContent).toContain('MODEL-15');
    const discount = rows[0].querySelector('input')!;
    discount.value = '50';
    discount.dispatchEvent(new Event('input'));
    fixture.detectChanges();
    expect(fixture.componentInstance.cartItems()[15].discount).toBe(50);
    expect(fixture.componentInstance.cartItems()[0].discount).toBe(0);
    expect(fixture.componentInstance.netAmount()).toBe(31950);
    (fixture.nativeElement.querySelector('.cart-table tbody tr .btn-remove-item') as HTMLButtonElement).click();
    fixture.detectChanges();
    expect(fixture.componentInstance.cartItems().some(row => row.inventoryItem.id === 'item-15')).toBe(false);
    expect(fixture.componentInstance.cartItems()[0].inventoryItem.id).toBe('item-0');
  });

  it('uses the same global row index for mobile cards', async () => {
    const fixture = await createCart();
    const first = fixture.nativeElement.querySelector('.cart-mobile-item') as HTMLElement;
    const discount = first.querySelector('input')!;
    discount.value = '75';
    discount.dispatchEvent(new Event('input'));
    fixture.detectChanges();
    expect(fixture.componentInstance.cartItems()[15].discount).toBe(75);
    (fixture.nativeElement.querySelector('.cart-mobile-item .btn-remove-item-mobile') as HTMLButtonElement).click();
    fixture.detectChanges();
    expect(fixture.componentInstance.cartItems().some(row => row.inventoryItem.id === 'item-15')).toBe(false);
  });
});
