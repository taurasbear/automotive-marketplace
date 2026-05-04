import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, it, expect, vi, beforeEach } from "vitest";
import EditListingForm from "./EditListingForm";
import type { GetListingByIdResponse } from "../types/GetListingByIdResponse";

vi.mock("react-i18next", () => ({
  useTranslation: () => ({ t: (key: string) => key }),
  initReactI18next: { type: "3rdParty", init: () => {} },
}));

vi.mock("@/components/forms/select/LocationCombobox", () => ({
  default: ({
    value,
    onValueChange,
  }: {
    value: string;
    onValueChange: (v: string) => void;
  }) => (
    <select
      data-testid="location-combobox"
      value={value}
      onChange={(e) => onValueChange(e.target.value)}
    >
      <option value="mun-1">Vilnius</option>
      <option value="mun-2">Kaunas</option>
    </select>
  ),
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
  municipalityId: "00000000-0000-0000-0000-000000000001",
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
  description: "A nice car",
  colour: "Blue",
  vin: "WBAPH5C55BA123456",
};

describe("EditListingForm", () => {
  const mockOnSubmit = vi.fn();

  beforeEach(() => {
    vi.clearAllMocks();
    mockOnSubmit.mockResolvedValue(undefined);
  });

  it("renders without crashing", () => {
    const { container } = render(
      <EditListingForm
        listing={baseListing}
        id="listing-1"
        onSubmit={mockOnSubmit}
      />,
    );
    expect(container.querySelector("form")).toBeInTheDocument();
  });

  it("renders year field pre-filled", () => {
    render(
      <EditListingForm
        listing={baseListing}
        id="listing-1"
        onSubmit={mockOnSubmit}
      />,
    );
    const yearInput = screen.getByLabelText("form.year");
    expect(yearInput).toHaveValue(2019);
  });

  it("renders price field pre-filled", () => {
    render(
      <EditListingForm
        listing={baseListing}
        id="listing-1"
        onSubmit={mockOnSubmit}
      />,
    );
    const priceInput = screen.getByLabelText("form.carPrice");
    expect(priceInput).toHaveValue(25000);
  });

  it("renders power field pre-filled", () => {
    render(
      <EditListingForm
        listing={baseListing}
        id="listing-1"
        onSubmit={mockOnSubmit}
      />,
    );
    const powerInput = screen.getByLabelText("form.enginePowerKw");
    expect(powerInput).toHaveValue(140);
  });

  it("renders mileage field pre-filled", () => {
    render(
      <EditListingForm
        listing={baseListing}
        id="listing-1"
        onSubmit={mockOnSubmit}
      />,
    );
    const mileageInput = screen.getByLabelText("form.mileage");
    expect(mileageInput).toHaveValue(120000);
  });

  it("renders description field pre-filled", () => {
    render(
      <EditListingForm
        listing={baseListing}
        id="listing-1"
        onSubmit={mockOnSubmit}
      />,
    );
    const descInput = screen.getByLabelText("form.descriptionLabel");
    expect(descInput).toHaveValue("A nice car");
  });

  it("renders submit button", () => {
    render(
      <EditListingForm
        listing={baseListing}
        id="listing-1"
        onSubmit={mockOnSubmit}
      />,
    );
    expect(
      screen.getByRole("button", { name: "edit.saveChanges" }),
    ).toBeInTheDocument();
  });

  it("calls onSubmit with form data on submit", async () => {
    const user = userEvent.setup();
    render(
      <EditListingForm
        listing={baseListing}
        id="listing-1"
        onSubmit={mockOnSubmit}
      />,
    );
    await user.click(
      screen.getByRole("button", { name: "edit.saveChanges" }),
    );
    await waitFor(
      () => {
        expect(mockOnSubmit).toHaveBeenCalled();
      },
      { timeout: 3000 },
    );
  });
});
