import { describe, it, expect } from "vitest";
import { computeDiff } from "./computeDiff";
import type { GetListingByIdResponse } from "@/features/listingDetails/types/GetListingByIdResponse";

const base: GetListingByIdResponse = {
  id: "a",
  makeName: "Toyota",
  modelName: "Camry",
  price: 15000,
  powerKw: 120,
  engineSizeMl: 1998,
  mileage: 50000,
  isSteeringWheelRight: false,
  municipalityId: "uuid-vilnius",
  municipalityName: "Vilnius",
  isUsed: true,
  year: 2020,
  transmissionName: "Automatic",
  fuelName: "Petrol",
  doorCount: 4,
  bodyTypeName: "Sedan",
  drivetrainName: "FWD",
  sellerName: "John",
  sellerId: "s1",
  status: "Available",
  images: [],
  defects: [],
};

describe("computeDiff", () => {
  it("returns equal for all fields when listings are identical", () => {
    const diff = computeDiff(base, { ...base });
    expect(diff.makeName).toBe("equal");
    expect(diff.price).toBe("equal");
    expect(diff.powerKw).toBe("equal");
    expect(diff.mileage).toBe("equal");
  });

  it("returns a-better when A has higher powerKw (higher-is-better)", () => {
    const b = { ...base, powerKw: 80 };
    const diff = computeDiff(base, b);
    expect(diff.powerKw).toBe("a-better");
  });

  it("returns b-better when B has higher powerKw (higher-is-better)", () => {
    const b = { ...base, powerKw: 200 };
    const diff = computeDiff(base, b);
    expect(diff.powerKw).toBe("b-better");
  });

  it("returns a-better when A has higher year (higher-is-better)", () => {
    const b = { ...base, year: 2015 };
    const diff = computeDiff(base, b);
    expect(diff.year).toBe("a-better");
  });

  it("returns b-better when B has higher year (higher-is-better)", () => {
    const b = { ...base, year: 2024 };
    const diff = computeDiff(base, b);
    expect(diff.year).toBe("b-better");
  });

  it("returns a-better when A has lower mileage (lower-is-better)", () => {
    const b = { ...base, mileage: 150000 };
    const diff = computeDiff(base, b);
    expect(diff.mileage).toBe("a-better");
  });

  it("returns b-better when B has lower mileage (lower-is-better)", () => {
    const b = { ...base, mileage: 10000 };
    const diff = computeDiff(base, b);
    expect(diff.mileage).toBe("b-better");
  });

  it("returns a-better when A has lower price (lower-is-better)", () => {
    const b = { ...base, price: 30000 };
    const diff = computeDiff(base, b);
    expect(diff.price).toBe("a-better");
  });

  it("returns b-better when B has lower price (lower-is-better)", () => {
    const b = { ...base, price: 5000 };
    const diff = computeDiff(base, b);
    expect(diff.price).toBe("b-better");
  });

  it("returns different for string field when values differ", () => {
    const b = { ...base, makeName: "Honda" };
    const diff = computeDiff(base, b);
    expect(diff.makeName).toBe("different");
  });

  it("returns equal for string field when values are the same", () => {
    const diff = computeDiff(base, { ...base });
    expect(diff.makeName).toBe("equal");
  });

  it("returns different for engineSizeMl when values differ (no direction)", () => {
    const b = { ...base, engineSizeMl: 2500 };
    const diff = computeDiff(base, b);
    expect(diff.engineSizeMl).toBe("different");
  });

  it("does not include images array in diff map", () => {
    const b = { ...base, images: [{ url: "x", altText: "y" }] };
    const diff = computeDiff(base, b);
    expect(diff.images).toBeUndefined();
  });
});
