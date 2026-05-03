import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, it, expect, vi, beforeEach } from "vitest";
import CreateListingForm from "./CreateListingForm";

const { mockCreateListingAsync, mockAddDefectAsync, mockAddDefectImageAsync } =
  vi.hoisted(() => ({
    mockCreateListingAsync: vi.fn(),
    mockAddDefectAsync: vi.fn(),
    mockAddDefectImageAsync: vi.fn(),
  }));

vi.mock("../api/useCreateListing", () => ({
  useCreateListing: () => ({ mutateAsync: mockCreateListingAsync }),
}));

vi.mock("@/api/defect/useAddListingDefect", () => ({
  useAddListingDefect: () => ({ mutateAsync: mockAddDefectAsync }),
}));

vi.mock("@/api/defect/useAddDefectImage", () => ({
  useAddDefectImage: () => ({ mutateAsync: mockAddDefectImageAsync }),
}));

vi.mock("react-i18next", () => ({
  useTranslation: () => ({
    t: (key: string) => key,
  }),
  initReactI18next: { type: "3rdParty", init: () => {} },
}));

vi.mock("@/components/forms/select/MakeSelect", () => ({
  default: ({
    onValueChange,
    value,
  }: {
    onValueChange: (v: string) => void;
    value?: string;
  }) => (
    <select
      data-testid="make-select"
      value={value ?? ""}
      onChange={(e) => onValueChange(e.target.value)}
    >
      <option value="">Select Make</option>
      <option value="make-1">Make 1</option>
    </select>
  ),
}));

vi.mock("@/components/forms/select/ModelSelect", () => ({
  default: ({
    onValueChange,
    disabled,
    value,
  }: {
    onValueChange: (v: string) => void;
    disabled?: boolean;
    value?: string;
  }) => (
    <select
      data-testid="model-select"
      disabled={disabled}
      value={value ?? ""}
      onChange={(e) => onValueChange(e.target.value)}
    >
      <option value="">Select Model</option>
      <option value="model-1">Model 1</option>
    </select>
  ),
}));

vi.mock("@/components/forms/VariantTable", () => ({
  default: ({
    modelId,
    onSelect,
  }: {
    modelId: string;
    selectedVariantId: string;
    onSelect: (v: null) => void;
  }) => (
    <div data-testid="variant-table" data-model-id={modelId}>
      <button onClick={() => onSelect(null)}>Clear Variant</button>
    </div>
  ),
}));

vi.mock("@/components/forms/select/FuelSelect", () => ({
  default: ({
    onValueChange,
    disabled,
  }: {
    onValueChange: (v: string) => void;
    disabled?: boolean;
  }) => (
    <select
      data-testid="fuel-select"
      disabled={disabled}
      onChange={(e) => onValueChange(e.target.value)}
    >
      <option value="">Fuel</option>
      <option value="fuel-1">Petrol</option>
    </select>
  ),
}));

vi.mock("@/components/forms/TransmissionToggleGroup", () => ({
  default: ({
    value,
    onValueChange,
    disabled,
  }: {
    value: string;
    onValueChange: (v: string) => void;
    disabled?: boolean;
    type: string;
  }) => (
    <div data-testid="transmission-toggle" data-disabled={disabled}>
      <button onClick={() => onValueChange("trans-1")}>{value || "Transmission"}</button>
    </div>
  ),
}));

vi.mock("@/components/forms/select/BodyTypeSelect", () => ({
  default: ({
    onValueChange,
    disabled,
  }: {
    onValueChange: (v: string) => void;
    disabled?: boolean;
  }) => (
    <select
      data-testid="body-type-select"
      disabled={disabled}
      onChange={(e) => onValueChange(e.target.value)}
    >
      <option value="">Body Type</option>
      <option value="body-1">Sedan</option>
    </select>
  ),
}));

vi.mock("@/components/forms/DrivetrainToggleGroup", () => ({
  default: ({
    value,
    onValueChange,
  }: {
    value: string;
    onValueChange: (v: string) => void;
    type: string;
  }) => (
    <div data-testid="drivetrain-toggle">
      <button onClick={() => onValueChange("drive-1")}>{value || "Drivetrain"}</button>
    </div>
  ),
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
      <option value="any">Any</option>
      <option value="mun-1">Vilnius</option>
    </select>
  ),
}));

vi.mock("@/components/forms/DefectSelector", () => ({
  default: ({
    selectedDefects,
    onDefectsChange,
  }: {
    mode: string;
    selectedDefects: unknown[];
    onDefectsChange: (d: unknown[]) => void;
  }) => (
    <div data-testid="defect-selector">
      <span data-testid="defect-count">{selectedDefects.length}</span>
      <button onClick={() => onDefectsChange([{ defectCategoryId: "cat-1", customName: "", images: [] }])}>
        Add Defect
      </button>
    </div>
  ),
}));

vi.mock("./ImageUploadInput", () => ({
  default: ({ field }: { field: { onChange: (v: Blob[]) => void; value: Blob[] } }) => (
    <input
      data-testid="image-upload"
      type="file"
      onChange={() =>
        field.onChange([...(field.value ?? []), new Blob(["img"], { type: "image/jpeg" })])
      }
    />
  ),
}));

vi.mock("./ImagePreview", () => ({
  default: ({
    images,
    onRemove,
  }: {
    images: Blob[];
    onRemove: (i: number) => void;
  }) => (
    <div data-testid="image-preview">
      <span data-testid="image-count">{images.length}</span>
      {images.length > 0 && (
        <button onClick={() => onRemove(0)}>Remove First</button>
      )}
    </div>
  ),
}));

vi.mock("browser-image-compression", () => ({
  default: vi.fn().mockResolvedValue(new File([""], "compressed.jpg")),
}));

describe("CreateListingForm", () => {
  beforeEach(() => {
    mockCreateListingAsync.mockReset();
    mockAddDefectAsync.mockReset();
    mockAddDefectImageAsync.mockReset();
  });

  it("renders without crashing", () => {
    render(<CreateListingForm />);
    expect(screen.getByTestId("make-select")).toBeInTheDocument();
  });

  it("renders all form sections", () => {
    render(<CreateListingForm />);

    expect(screen.getByTestId("make-select")).toBeInTheDocument();
    expect(screen.getByTestId("model-select")).toBeInTheDocument();
    expect(screen.getByTestId("variant-table")).toBeInTheDocument();
    expect(screen.getByTestId("fuel-select")).toBeInTheDocument();
    expect(screen.getByTestId("transmission-toggle")).toBeInTheDocument();
    expect(screen.getByTestId("body-type-select")).toBeInTheDocument();
    expect(screen.getByTestId("drivetrain-toggle")).toBeInTheDocument();
    expect(screen.getByTestId("location-combobox")).toBeInTheDocument();
    expect(screen.getByTestId("defect-selector")).toBeInTheDocument();
    expect(screen.getByTestId("image-upload")).toBeInTheDocument();
    expect(screen.getByTestId("image-preview")).toBeInTheDocument();
  });

  it("renders price, mileage, year, colour, vin, and description fields", () => {
    render(<CreateListingForm />);

    expect(screen.getByText("form.carPrice")).toBeInTheDocument();
    expect(screen.getByText("form.mileage")).toBeInTheDocument();
    expect(screen.getByText("form.year")).toBeInTheDocument();
    expect(screen.getByText("form.colour")).toBeInTheDocument();
    expect(screen.getByText("form.vinLabel")).toBeInTheDocument();
    expect(screen.getByText("form.descriptionLabel")).toBeInTheDocument();
  });

  it("renders submit button", () => {
    render(<CreateListingForm />);
    expect(screen.getByRole("button", { name: "form.submit" })).toBeInTheDocument();
  });

  it("disables submit button when submitDisabled is true", () => {
    render(<CreateListingForm submitDisabled={true} />);
    expect(screen.getByRole("button", { name: "form.submit" })).toBeDisabled();
  });

  it("enables submit button when submitDisabled is false", () => {
    render(<CreateListingForm submitDisabled={false} />);
    expect(screen.getByRole("button", { name: "form.submit" })).toBeEnabled();
  });

  it("model select is disabled when no make is selected", () => {
    render(<CreateListingForm />);
    expect(screen.getByTestId("model-select")).toBeDisabled();
  });

  it("model select is enabled when a make is selected", async () => {
    const user = userEvent.setup();
    render(<CreateListingForm />);

    await user.selectOptions(screen.getByTestId("make-select"), "make-1");
    expect(screen.getByTestId("model-select")).not.toBeDisabled();
  });

  it("renders checkbox fields for steering wheel and used car", () => {
    render(<CreateListingForm />);
    expect(screen.getByText("form.steeringWheelRight")).toBeInTheDocument();
    expect(screen.getByText("form.usedCar")).toBeInTheDocument();
  });

  it("renders image upload section and preview", () => {
    render(<CreateListingForm />);
    expect(screen.getByTestId("image-upload")).toBeInTheDocument();
    expect(screen.getByTestId("image-preview")).toBeInTheDocument();
    expect(screen.getByTestId("image-count")).toHaveTextContent("0");
  });

  it("renders defect selector in form mode", () => {
    render(<CreateListingForm />);
    expect(screen.getByTestId("defect-selector")).toBeInTheDocument();
    expect(screen.getByTestId("defect-count")).toHaveTextContent("0");
  });

  it("renders specifications section with lock/unlock labels", () => {
    render(<CreateListingForm />);
    expect(screen.getByText("form.specifications")).toBeInTheDocument();
    expect(screen.getByText("form.fuelType")).toBeInTheDocument();
    expect(screen.getByText("form.transmission")).toBeInTheDocument();
    expect(screen.getByText("form.bodyType")).toBeInTheDocument();
    expect(screen.getByText("form.enginePowerKw")).toBeInTheDocument();
    expect(screen.getByText("form.engineSizeMl")).toBeInTheDocument();
    expect(screen.getByText("form.doorCount")).toBeInTheDocument();
  });
});
