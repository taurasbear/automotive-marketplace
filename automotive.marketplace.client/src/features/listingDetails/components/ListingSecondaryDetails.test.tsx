import { render, screen } from "@testing-library/react";
import { describe, it, expect, vi } from "vitest";
import { ListingSecondaryDetails } from "./ListingSecondaryDetails";
import type { GetListingByIdResponse } from "../types/GetListingByIdResponse";

vi.mock("react-i18next", () => ({
  useTranslation: () => ({ t: (key: string) => key }),
}));

vi.mock("@/features/listingList/utils/translateVehicleAttr", () => ({
  translateVehicleAttr: (_type: string, value: string) => value,
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

describe("ListingSecondaryDetails", () => {
  it("renders without crashing", () => {
    render(<ListingSecondaryDetails listing={baseListing} />);
    expect(screen.getByText("details.additionalDetails")).toBeInTheDocument();
  });

  it("renders body type", () => {
    render(<ListingSecondaryDetails listing={baseListing} />);
    expect(screen.getByText("Sedan")).toBeInTheDocument();
  });

  it("renders door count", () => {
    render(<ListingSecondaryDetails listing={baseListing} />);
    expect(screen.getByText("4")).toBeInTheDocument();
  });

  it("renders left-hand steering when isSteeringWheelRight is false", () => {
    render(<ListingSecondaryDetails listing={baseListing} />);
    expect(screen.getByText("details.leftHand")).toBeInTheDocument();
  });

  it("renders right-hand steering when isSteeringWheelRight is true", () => {
    render(
      <ListingSecondaryDetails
        listing={{ ...baseListing, isSteeringWheelRight: true }}
      />,
    );
    expect(screen.getByText("details.rightHand")).toBeInTheDocument();
  });

  it("renders seller name", () => {
    render(<ListingSecondaryDetails listing={baseListing} />);
    expect(screen.getByText("John")).toBeInTheDocument();
  });

  it("renders colour when present", () => {
    render(
      <ListingSecondaryDetails listing={{ ...baseListing, colour: "Blue" }} />,
    );
    expect(screen.getByText("Blue")).toBeInTheDocument();
  });

  it("does not render colour row when absent", () => {
    render(<ListingSecondaryDetails listing={baseListing} />);
    expect(screen.queryByText("details.colour")).not.toBeInTheDocument();
  });

  it("renders VIN when present", () => {
    render(
      <ListingSecondaryDetails
        listing={{ ...baseListing, vin: "WBA123456789" }}
      />,
    );
    expect(screen.getByText("WBA123456789")).toBeInTheDocument();
  });

  it("does not render VIN row when absent", () => {
    render(<ListingSecondaryDetails listing={baseListing} />);
    expect(screen.queryByText("details.vin")).not.toBeInTheDocument();
  });

  it("renders market median price when present", () => {
    render(
      <ListingSecondaryDetails
        listing={{ ...baseListing, marketMedianPrice: 23000 }}
      />,
    );
    expect(screen.getByText("23000 €")).toBeInTheDocument();
  });

  it("renders safety rating when present", () => {
    render(
      <ListingSecondaryDetails
        listing={{ ...baseListing, safetyRating: 4 }}
      />,
    );
    expect(screen.getByText("4/5 ⭐")).toBeInTheDocument();
  });
});
