// Centralized mock data for Playwright fixtures (TypeScript)
export const mockInputRoot = 'tests/TestData/205196000';

export const mockVessel205196000 = {
  mmsi: '205196000',
  displayName: 'Test Vessel 205196000',
  minDate: '2025-03-15T00:00:00Z',
  maxDate: '2025-03-15T23:59:59Z'
};

export function makeCsvRow(ts: string, lat?: number, lon?: number, sog?: number, heading?: number) {
  // Simplified CSV row: timestamp,latitude,longitude,sog,heading
  return `${ts},${lat ?? ''},${lon ?? ''},${sog ?? ''},${heading ?? ''}\n`;
}
