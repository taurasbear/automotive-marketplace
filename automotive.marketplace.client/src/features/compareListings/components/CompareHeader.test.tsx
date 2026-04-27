import { render, screen, fireEvent } from "@testing-library/react";
import { describe, it, expect, vi } from "vitest";
import { CompareHeader } from "./CompareHeader";
import type { GetListingByIdResponse } from "@/features/listingDetails/types/GetListingByIdResponse";

vi.mock("@tanstack/react-router", async (importOriginal) => {
  const actual = await importOriginal<typeof import("@tanstack/react-router")>();
  return {
    ...actual,
    Link: ({ children, className }: { children: React.ReactNode; className?: string }) => (
      <a className={className}>{children}</a>
    ),
  };
});

vi.mock("react-i18next", () => ({
  useTranslation: () => ({
    t: (key: string) => {
      const translations: Record<string, string> = {
        "header.specification": "Specification",
        "header.change": "Change",
        "header.changeListingA": "Change listing A",
        "header.changeListingB": "Change listing B",
      };
      return translations[key] || key;
    },
  }),
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
  modelName: "Civic",
  sellerId: "s2",
};

describe("CompareHeader — Change buttons", () => {
  it("does not render any Change button when no onChange callbacks are provided", () => {
    render(<CompareHeader listingA={listingA} listingB={listingB} />);
    expect(screen.queryByRole("button")).not.toBeInTheDocument();
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
      screen.getByRole("button", { name: "Change listing A" }),
    ).toBeInTheDocument();
  });

  it("renders a Change button for listing B when onChangeB is provided", () => {
    render(
      <CompareHeader
        listingA={listingA}
        listingB={listingB}
        onChangeB={vi.fn()}
      />,
    );
    expect(
      screen.getByRole("button", { name: "Change listing B" }),
    ).toBeInTheDocument();
  });

  it("calls onChangeA when the first Change button is clicked", () => {
    const onChangeA = vi.fn();
    const onChangeB = vi.fn();
    render(
      <CompareHeader
        listingA={listingA}
        listingB={listingB}
        onChangeA={onChangeA}
        onChangeB={onChangeB}
      />,
    );

    fireEvent.click(screen.getByRole("button", { name: "Change listing A" }));
    expect(onChangeA).toHaveBeenCalledTimes(1);
    expect(onChangeB).not.toHaveBeenCalled();
  });

  it("calls onChangeB when the second Change button is clicked", () => {
    const onChangeA = vi.fn();
    const onChangeB = vi.fn();
    render(
      <CompareHeader
        listingA={listingA}
        listingB={listingB}
        onChangeA={onChangeA}
        onChangeB={onChangeB}
      />,
    );

    fireEvent.click(screen.getByRole("button", { name: "Change listing B" }));
    expect(onChangeB).toHaveBeenCalledTimes(1);
    expect(onChangeA).not.toHaveBeenCalled();
  });
});
