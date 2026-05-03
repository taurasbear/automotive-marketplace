import { render, screen, fireEvent } from "@testing-library/react";
import { describe, it, expect, vi, beforeEach, afterEach } from "vitest";
import NoteEditor from "./NoteEditor";
import type { SavedListing } from "../types/SavedListing";

const { mockUpsertMutate, mockDeleteMutate } = vi.hoisted(() => ({
  mockUpsertMutate: vi.fn(),
  mockDeleteMutate: vi.fn(),
}));

vi.mock("../api/useUpsertListingNote", () => ({
  useUpsertListingNote: () => ({ mutate: mockUpsertMutate }),
}));

vi.mock("../api/useDeleteListingNote", () => ({
  useDeleteListingNote: () => ({ mutate: mockDeleteMutate }),
}));

vi.mock("react-i18next", () => ({
  useTranslation: () => ({ t: (key: string) => key }),
}));

vi.mock("@/lib/i18n/formatNumber", () => ({
  formatCurrency: (v: number) => v.toString(),
  formatNumber: (v: number) => v.toString(),
}));

const createListing = (overrides: Partial<SavedListing> = {}): SavedListing => ({
  listingId: "listing-1",
  title: "2020 BMW X5",
  price: 35000,
  municipalityName: "Vilnius",
  mileage: 80000,
  fuelName: "Diesel",
  transmissionName: "Automatic",
  thumbnail: null,
  noteContent: null,
  ...overrides,
});

describe("NoteEditor", () => {
  beforeEach(() => {
    vi.useFakeTimers();
    mockUpsertMutate.mockReset();
    mockDeleteMutate.mockReset();
  });

  afterEach(() => {
    vi.useRealTimers();
  });

  it("returns null when collapsed and no note content", () => {
    const { container } = render(
      <NoteEditor listing={createListing()} isExpanded={false} />,
    );
    expect(container.innerHTML).toBe("");
  });

  it("shows note preview when collapsed and note content exists", () => {
    render(
      <NoteEditor
        listing={createListing({ noteContent: "My saved note" })}
        isExpanded={false}
      />,
    );
    expect(screen.getByText("My saved note")).toBeInTheDocument();
  });

  it("shows placeholder when expanded and no note content", () => {
    render(<NoteEditor listing={createListing()} isExpanded={true} />);
    expect(screen.getByText("noteEditor.placeholder")).toBeInTheDocument();
  });

  it("shows existing note content when expanded", () => {
    render(
      <NoteEditor
        listing={createListing({ noteContent: "Existing note" })}
        isExpanded={true}
      />,
    );
    expect(screen.getByText("Existing note")).toBeInTheDocument();
  });

  it("enters edit mode on click and shows textarea with content", () => {
    render(
      <NoteEditor
        listing={createListing({ noteContent: "Edit me" })}
        isExpanded={true}
      />,
    );

    fireEvent.click(screen.getByText("Edit me"));
    expect(screen.getByRole("textbox")).toHaveValue("Edit me");
  });

  it("typing triggers debounced upsert save", () => {
    render(
      <NoteEditor listing={createListing()} isExpanded={true} />,
    );

    // Click placeholder to enter edit mode
    fireEvent.click(screen.getByText("noteEditor.placeholder"));

    const textarea = screen.getByRole("textbox");
    fireEvent.change(textarea, { target: { value: "Hello" } });

    // Should not have been called yet
    expect(mockUpsertMutate).not.toHaveBeenCalled();

    // Advance past debounce time
    vi.advanceTimersByTime(800);

    expect(mockUpsertMutate).toHaveBeenCalledWith({
      listingId: "listing-1",
      content: "Hello",
    });
  });

  it("auto-saves on unmount when there is pending text", () => {
    const { unmount } = render(
      <NoteEditor listing={createListing()} isExpanded={true} />,
    );

    fireEvent.click(screen.getByText("noteEditor.placeholder"));

    const textarea = screen.getByRole("textbox");
    fireEvent.change(textarea, { target: { value: "Unsaved text" } });

    // Don't advance timers — unmount while debounce is pending
    unmount();

    expect(mockUpsertMutate).toHaveBeenCalledWith({
      listingId: "listing-1",
      content: "Unsaved text",
    });
  });

  it("calls deleteNote when text is cleared and noteContent existed", () => {
    render(
      <NoteEditor
        listing={createListing({ noteContent: "Old note" })}
        isExpanded={true}
      />,
    );

    fireEvent.click(screen.getByText("Old note"));

    const textarea = screen.getByRole("textbox");
    fireEvent.change(textarea, { target: { value: "" } });

    vi.advanceTimersByTime(800);

    expect(mockDeleteMutate).toHaveBeenCalledWith({
      listingId: "listing-1",
    });
  });
});
