import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { MasterDataService } from './master-data.service';

describe('MasterDataService API handling', () => {
  let service: MasterDataService;
  let http: HttpTestingController;
  beforeEach(() => {
    TestBed.configureTestingModule({ providers: [provideHttpClient(), provideHttpClientTesting()] });
    service = TestBed.inject(MasterDataService);
    http = TestBed.inject(HttpTestingController);
  });
  afterEach(() => http.verify());

  it('converts category and customer types for numeric edit controls', () => {
    service.loadCategories().subscribe();
    http.expectOne('/api/master-data/categories').flush({ data: [{ type: 'Lens' }] });
    expect(service.categories()[0].type).toBe(2);
    service.loadCustomers().subscribe();
    http.expectOne('/api/customers').flush({ data: [{ customerType: 'Both' }] });
    expect(service.customers()[0].customerType).toBe(3);
  });

  it('clears the loading indicator when a request fails', () => {
    service.loadBrands().subscribe({ error: () => {} });
    expect(service.loading()).toBe(true);
    http.expectOne('/api/master-data/brands').flush({}, { status: 500, statusText: 'Server Error' });
    expect(service.loading()).toBe(false);
  });
});
