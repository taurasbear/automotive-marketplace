import { render, screen } from "@testing-library/react";
import { describe, it, expect, vi } from "vitest";
import { ListingKeySpecs } from "./ListingKeySpecs";
import type { GetListingByIdResponse } from "../types/GetListingByIdResponse";

vi.mock("react-i18next", () => ({
  useTranslation: () => ({ t: (key: string) => key }),
}));

vi.mock("@/features/listingList", () => ({
  ListingCardBadge: ({
    title,
    stat,
  }: {
    title: string;
    stat: string;
    Icon: React.ReactNode;
  }) => (
    <div data-testid={`badge-${title}`}>
      <span>{title}</span>
      <span>{stat}</span>
    </div>
  ),
}));

vi.mock("@/features/listingList/utils/translateVehicleAttr", () => ({
  translateVehicleAttr: (_type: string, value: string) => value,
}));

vi.mock("@/lib/i18n/formatNumber", () => ({
  formatNumber: (v: number) => v.toLocaleString(),
}));

const baseListing: GetListingByIdResponse = {
  id: "listing-1",
  makeName: "BMW",
  modelName: "3 Series",
  price: 25000,
  powerKw: 140,
  engineSizeMl: 2000,
  mileage: 120000,
  isSteeringWheelRight: false,
  municipalityId: "mun-1",
  municipalityName: "Vilnius",
  isUsed: true,
  year: 2019,
  transmissionName: "Manual",
  fuelName: "Diesel",
  doorCount: 4,
  bodyTypeName: "Sedan",
  drivetrainName: "RWD",
  sellerName: "John",
  sellerId: "seller-1",
  status: "Active",
  images: [],
  defects: [],
};

describe("ListingKeySpecs", () => {
  it("renders without crashing", () => {
    render(<ListingKeySpecs listing={baseListing} />);
    expect(screen.getByText("details.keySpecs")).toBeInTheDocument();
  });

  it("renders engine spec with size and power", () => {
    render(<ListingKeySpecs listing={baseListing} />);
    expect(screen.getByText("2 l 140 kW")).toBeInTheDocument();
  });

  it("renders fuel type", () => {
    render(<ListingKeySpecs listing={baseListing} />);
    expect(screen.getByText("Diesel")).toBeInTheDocument();
  });

  it("renders transmission", () => {
    render(<ListingKeySpecs listing={baseListing} />);
    expect(screen.getByText("Manual")).toBeInTheDocument();
  });

  it("renders mileage with km suffix", () => {
    render(<ListingKeySpecs listing={baseListing} />);
    expect(screen.getByText("120,000 km")).toBeInTheDocument();
  });

  it("renders year", () => {
    render(<ListingKeySpecs listing={baseListing} />);
    expect(screen.getByText("2019")).toBeInTheDocument();
  });

  it("renders drivetrain", () => {
    render(<ListingKeySpecs listing={baseListing} />);
    expect(screen.getByText("RWD")).toBeInTheDocument();
  });
});
