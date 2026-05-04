import { render, screen } from "@testing-library/react";
import { describe, it, expect, vi } from "vitest";
import { CompareTable } from "./CompareTable";
import type { GetListingByIdResponse } from "@/features/listingDetails/types/GetListingByIdResponse";
import type { DiffMap } from "../types/diff";

vi.mock("react-i18next", () => ({
  useTranslation: () => ({ t: (key: string) => key }),
  initReactI18next: { type: "3rdParty", init: () => {} },
}));

const listingA: GetListingByIdResponse = {
  id: "a1",
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

const listingB: GetListingByIdResponse = {
  ...listingA,
  id: "b1",
  makeName: "Honda",
  powerKw: 100,
  mileage: 80000,
};

const allEqualDiff: DiffMap = {
  makeName: "equal",
  modelName: "equal",
  bodyTypeName: "equal",
  year: "equal",
  isUsed: "equal",
  mileage: "equal",
  city: "equal",
  powerKw: "equal",
  engineSizeMl: "equal",
  fuelName: "equal",
  transmissionName: "equal",
  drivetrainName: "equal",
  price: "equal",
  status: "equal",
  sellerName: "equal",
};

const mixedDiff: DiffMap = {
  ...allEqualDiff,
  makeName: "different",
  powerKw: "a-better",
  mileage: "a-better",
};

describe("CompareTable", () => {
  it("renders all section headings when diffOnly is false", () => {
    render(
      <CompareTable
        listingA={listingA}
        listingB={listingB}
        diffMap={allEqualDiff}
        diffOnly={false}
      />,
    );
    expect(screen.getByText("table.basicInfo")).toBeInTheDocument();
    expect(screen.getByText("table.engineAndPerformance")).toBeInTheDocument();
    expect(screen.getByText("table.listingDetails")).toBeInTheDocument();
  });

  it("renders row labels for all spec fields when diffOnly is false", () => {
    render(
      <CompareTable
        listingA={listingA}
        listingB={listingB}
        diffMap={allEqualDiff}
        diffOnly={false}
      />,
    );
    expect(screen.getByText("table.make")).toBeInTheDocument();
    expect(screen.getByText("table.powerKw")).toBeInTheDocument();
    expect(screen.getByText("table.price")).toBeInTheDocument();
  });

  it("hides rows where diff is equal when diffOnly is true", () => {
    render(
      <CompareTable
        listingA={listingA}
        listingB={listingB}
        diffMap={mixedDiff}
        diffOnly={true}
      />,
    );
    expect(screen.queryByText("table.model")).not.toBeInTheDocument();
    expect(screen.getByText("table.make")).toBeInTheDocument();
    expect(screen.getByText("table.powerKw")).toBeInTheDocument();
    expect(screen.getByText("table.mileage")).toBeInTheDocument();
  });

  it("applies green class to the better cell for a-better numeric field", () => {
    render(
      <CompareTable
        listingA={listingA}
        listingB={listingB}
        diffMap={mixedDiff}
        diffOnly={false}
      />,
    );
    // powerKw: a-better — A cell (120 kW) should have green class
    const cells = screen.getAllByText(/120/);
    const betterCell = cells[0].closest("td");
    expect(betterCell?.className).toMatch(/text-green/);
  });

  it("applies orange class to the worse cell for a-better numeric field", () => {
    render(
      <CompareTable
        listingA={listingA}
        listingB={listingB}
        diffMap={mixedDiff}
        diffOnly={false}
      />,
    );
    // powerKw: a-better — B cell (100 kW) should have orange class
    const cells = screen.getAllByText(/100/);
    const worseCell = cells[0].closest("td");
    expect(worseCell?.className).toMatch(/text-orange/);
  });
});
