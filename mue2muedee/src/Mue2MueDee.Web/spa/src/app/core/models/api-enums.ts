// ASP.NET returns enum names. Convert them at the API boundary for numeric form controls.
export function numericEnumValue<T extends number>(value: T | string, values: Record<string, string | number>): T {
  const numeric = typeof value === 'number' ? value : values[value];
  if (typeof numeric !== 'number' || !Object.values(values).includes(numeric)) {
    throw new Error(`Unknown API enum value: ${value}`);
  }
  return numeric as T;
}

export const categoryTypeValues = { Camera: 1, Lens: 2, Flash: 3, Accessory: 4, Other: 99 };
export const customerTypeValues = { Seller: 1, Buyer: 2, Both: 3 };
export const stockMovementTypeValues = {
  Receive: 1, Reserve: 2, CancelReservation: 3, Sell: 4,
  Return: 5, StatusChange: 6, CostAdjustment: 7, WriteOff: 8
};
