import { render, screen, fireEvent } from "@testing-library/react";
import { describe, it, expect, vi } from "vitest";
import { CompareHeader } from "./CompareHeader";
import type { GetListingByIdResponse } from "@/features/listingDetails/types/GetListingByIdResponse";

const listingA: GetListingByIdResponse = {
  id: "a1",
  makeName: "Toyota",
  modelName: "Camry",
  price: 15000,
  powerKw: 120,
  engineSizeMl: 1998,
  mileage: 50000,
  isSteeringWheelRight: false,
  city: "Vilnius",
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
};

const listingB: GetListingByIdResponse = {
  ...listingA,
  id: "b1",
  makeName: "Honda",
  modelName: "Civic",
};

describe("CompareHeader — Change buttons", () => {
  it("does not render any Change button when no onChange callbacks are provided", () => {
    render(<CompareHeader listingA={listingA} listingB={listingB} />);
    expect(
      screen.queryByRole("button", { name: "Change" }),
    ).not.toBeInTheDocument();
  });

  it("renders a Change button for listing A when onChangeA is provided", () => {
    render(
      <CompareHeader
        listingA={listingA}
        listingB={listingB}
        onChangeA={vi.fn()}
      />,
    );
    expect(
      screen.getByRole("button", { name: "Change" }),
    ).toBeInTheDocument();
  });

  it("calls onChangeA when the first Change button is clicked", () => {
    const onChangeA = vi.fn();
    render(
      <CompareHeader
        listingA={listingA}
        listingB={listingB}
        onChangeA={onChangeA}
        onChangeB={vi.fn()}
      />,
    );

    const buttons = screen.getAllByRole("button", { name: "Change" });
    fireEvent.click(buttons[0]);
    expect(onChangeA).toHaveBeenCalledTimes(1);
  });

  it("calls onChangeB when the second Change button is clicked", () => {
    const onChangeB = vi.fn();
    render(
      <CompareHeader
        listingA={listingA}
        listingB={listingB}
        onChangeA={vi.fn()}
        onChangeB={onChangeB}
      />,
    );

    const buttons = screen.getAllByRole("button", { name: "Change" });
    fireEvent.click(buttons[1]);
    expect(onChangeB).toHaveBeenCalledTimes(1);
  });
});
