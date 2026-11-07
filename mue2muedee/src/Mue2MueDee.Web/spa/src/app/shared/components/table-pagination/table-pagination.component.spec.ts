import { TestBed } from '@angular/core/testing';
import { TablePaginationComponent, TablePageChange } from './table-pagination.component';

describe('TablePaginationComponent', () => {
  function create() {
    const fixture = TestBed.createComponent(TablePaginationComponent<{ id: number }>);
    return { fixture, pager: fixture.componentInstance };
  }

  it('shows page slices and preserves global row indices for actions', () => {
    const { fixture, pager } = create();
    fixture.componentRef.setInput('items', Array.from({ length: 34 }, (_, id) => ({ id })));
    fixture.detectChanges();
    expect(pager.pagedItems().length).toBe(15);
    pager.changePage(2);
    expect(pager.startIndex()).toBe(15);
    expect(pager.pagedItems()[0].id).toBe(15);
    pager.changePage(3);
    expect(pager.pagedItems().length).toBe(4);
    expect(pager.rangeEnd()).toBe(34);
    expect(pager.items().length).toBe(34);
  });

  it('resets filtered data to page one without resetting identical array contents', () => {
    const { fixture, pager } = create();
    const rows = Array.from({ length: 45 }, (_, id) => ({ id }));
    fixture.componentRef.setInput('items', rows);
    fixture.detectChanges();
    pager.changePage(2);
    fixture.componentRef.setInput('items', [...rows]);
    fixture.detectChanges();
    expect(pager.currentPage()).toBe(2);
    fixture.componentRef.setInput('items', rows.map(row => ({ ...row })));
    fixture.detectChanges();
    expect(pager.currentPage()).toBe(2);
    fixture.componentRef.setInput('items', rows.filter(row => row.id > 10));
    fixture.detectChanges();
    expect(pager.currentPage()).toBe(1);
  });

  it('changes page size through the reactive form and resets the current page', () => {
    const { fixture, pager } = create();
    fixture.componentRef.setInput('items', Array.from({ length: 75 }, (_, id) => ({ id })));
    fixture.detectChanges();
    pager.changePage(4);
    pager.sizeControl.setValue('25');
    expect(pager.currentPage()).toBe(1);
    expect(pager.pagedItems().length).toBe(25);
  });

  it('requests server pages without slicing server results a second time', () => {
    const { fixture, pager } = create();
    const events: TablePageChange[] = [];
    pager.paginationChange.subscribe(event => events.push(event));
    fixture.componentRef.setInput('items', [{ id: 16 }]);
    fixture.componentRef.setInput('totalCount', 100);
    fixture.componentRef.setInput('page', 2);
    fixture.componentRef.setInput('pageSize', 15);
    fixture.detectChanges();
    expect(pager.pagedItems()).toEqual([{ id: 16 }]);
    pager.changePage(3);
    pager.changeSize(50);
    expect(events).toEqual([{ page: 3, pageSize: 15 }, { page: 1, pageSize: 50 }]);
  });

  it('handles empty results and disables invalid navigation', () => {
    const { fixture, pager } = create();
    fixture.detectChanges();
    expect(pager.rangeStart()).toBe(0);
    expect(pager.rangeEnd()).toBe(0);
    expect(pager.totalPages()).toBe(1);
    expect(fixture.nativeElement.textContent).toContain('แสดง 0–0 จาก 0 รายการ');
    const previous = fixture.nativeElement.querySelector('[aria-label="หน้าก่อนหน้า"]') as HTMLButtonElement;
    const next = fixture.nativeElement.querySelector('[aria-label="หน้าถัดไป"]') as HTMLButtonElement;
    expect(previous.disabled).toBe(true);
    expect(next.disabled).toBe(true);
  });

  it('does not emit another request while a server page is loading', () => {
    const { fixture, pager } = create();
    const events: TablePageChange[] = [];
    pager.paginationChange.subscribe(event => events.push(event));
    fixture.componentRef.setInput('totalCount', 100);
    fixture.componentRef.setInput('disabled', true);
    fixture.detectChanges();
    pager.changePage(2);
    pager.changeSize(25);
    expect(events).toEqual([]);
    expect(pager.sizeControl.disabled).toBe(true);
  });
});
