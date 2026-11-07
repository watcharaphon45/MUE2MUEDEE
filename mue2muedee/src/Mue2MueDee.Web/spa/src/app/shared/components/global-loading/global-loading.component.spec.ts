import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { App } from '../../../app';
import { LoadingService } from '../../../core/loading/loading.service';

describe('Global loading screen', () => {
  function setup() {
    TestBed.configureTestingModule({ providers: [provideRouter([])] });
    const fixture = TestBed.createComponent(App);
    fixture.detectChanges();
    TestBed.tick();
    return { fixture, loading: TestBed.inject(LoadingService) };
  }

  it('blocks page content while requests remain and restores it after the final release', () => {
    const { fixture, loading } = setup();
    const first = loading.begin();
    const second = loading.begin();
    TestBed.tick();
    const content = fixture.nativeElement.querySelector('.application-content') as HTMLElement;
    expect(content.inert).toBe(true);
    expect(content.getAttribute('aria-busy')).toBe('true');
    expect(document.querySelector('.loading-card')?.textContent).toContain('กำลังดำเนินการ');
    first();
    TestBed.tick();
    expect(document.querySelector('.loading-card')).not.toBeNull();
    second();
    TestBed.tick();
    expect(document.querySelector('.loading-card')).toBeNull();
    expect(content.inert).toBe(false);
    expect(content.getAttribute('aria-busy')).toBe('false');
  });

  it('cannot be dismissed by Escape or backdrop and blocks page keyboard shortcuts', () => {
    const { loading } = setup();
    const finish = loading.begin();
    TestBed.tick();
    const shortcut = vi.fn();
    document.addEventListener('keydown', shortcut);
    document.body.dispatchEvent(new KeyboardEvent('keydown', { key: 'Escape', keyCode: 27, bubbles: true }));
    (document.querySelector('.global-loading-backdrop') as HTMLElement).click();
    TestBed.tick();
    expect(document.querySelector('.loading-card')).not.toBeNull();
    expect(shortcut).not.toHaveBeenCalled();
    finish();
    TestBed.tick();
    document.body.dispatchEvent(new KeyboardEvent('keydown', { key: 'x', bubbles: true }));
    expect(shortcut).toHaveBeenCalledOnce();
    document.removeEventListener('keydown', shortcut);
  });
});
