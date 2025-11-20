export interface ShipDataOut {
  BaseDateTime: string;
  Lat?: number;
  Lon?: number;
  SOG?: number;
  Heading?: number;
  EtaSecondsUntil?: number;
}

export const sampleShipDataOut: ShipDataOut[] = [
  { BaseDateTime: '2025-03-15T00:00:00Z', Lat: 59.123456, Lon: 10.123456, SOG: 10, Heading: 90, EtaSecondsUntil: 3600 },
  { BaseDateTime: '2025-03-15T00:05:00Z', Lat: 59.123556, Lon: 10.123556, SOG: 12, Heading: 95 }
];
