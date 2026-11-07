import { numericEnumValue, customerTypeValues } from './api-enums';
import { InventoryStatus } from '../../features/inventory/inventory.models';

describe('API enum conversion', () => {
  it('converts named enums and retains numeric values', () => {
    expect(numericEnumValue('Reserved', InventoryStatus)).toBe(InventoryStatus.Reserved);
    expect(numericEnumValue(InventoryStatus.Available, InventoryStatus)).toBe(InventoryStatus.Available);
    expect(numericEnumValue('Both', customerTypeValues)).toBe(3);
  });

  it('rejects unknown statuses instead of silently hiding workflow controls', () => {
    expect(() => numericEnumValue('Unknown', InventoryStatus)).toThrow();
  });
});
