import { apiDateBoundary, businessDateInput } from './business-date';

describe('Thai business date filters', () => {
  it('uses the Bangkok date across UTC month boundaries', () => {
    expect(businessDateInput(new Date('2026-10-31T18:00:00Z'))).toBe('2026-11-01');
  });

  it('includes the whole selected Thai end date', () => {
    expect(new Date(apiDateBoundary('2026-11-01')).toISOString()).toBe('2026-10-31T17:00:00.000Z');
    expect(new Date(apiDateBoundary('2026-11-01', true)).toISOString()).toBe('2026-11-01T16:59:59.999Z');
    expect(apiDateBoundary('2026-11-01T12:00:00Z', true)).toBe('2026-11-01T12:00:00Z');
  });
});
