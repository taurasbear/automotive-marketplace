import type { TFunction } from "i18next";

export type VehicleAttrType =
  | "fuel"
  | "transmission"
  | "drivetrain"
  | "bodyType"
  | "status";

export function translateVehicleAttr(
  type: VehicleAttrType,
  value: string,
  t: TFunction<"listings">,
): string {
  const key = `vehicleAttributes.${type}.${value}` as Parameters<typeof t>[0];
  const translated = t(key);
  return translated === key ? value : translated;
}
