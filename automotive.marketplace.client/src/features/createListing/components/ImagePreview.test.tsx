import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, it, expect, vi, beforeEach } from "vitest";
import ImagePreview from "./ImagePreview";

const mockCreateObjectURL = vi.fn(() => "blob:mock-url");
const mockRevokeObjectURL = vi.fn();

beforeEach(() => {
  global.URL.createObjectURL = mockCreateObjectURL;
  global.URL.revokeObjectURL = mockRevokeObjectURL;
  mockCreateObjectURL.mockClear();
  mockRevokeObjectURL.mockClear();
});

describe("ImagePreview", () => {
  const mockOnRemove = vi.fn();

  beforeEach(() => {
    mockOnRemove.mockReset();
  });

  it("renders nothing when images array is empty", () => {
    const { container } = render(
      <ImagePreview images={[]} onRemove={mockOnRemove} />,
    );
    expect(container.innerHTML).toBe("");
  });

  it("renders image thumbnails from blob URLs", () => {
    const images = [
      new Blob(["img1"], { type: "image/jpeg" }),
      new Blob(["img2"], { type: "image/png" }),
    ];
    mockCreateObjectURL
      .mockReturnValueOnce("blob:url-1")
      .mockReturnValueOnce("blob:url-2");

    render(<ImagePreview images={images} onRemove={mockOnRemove} />);

    const imgs = screen.getAllByRole("img");
    expect(imgs).toHaveLength(2);
    expect(imgs[0]).toHaveAttribute("src", "blob:url-1");
    expect(imgs[1]).toHaveAttribute("src", "blob:url-2");
    expect(imgs[0]).toHaveAttribute("alt", "Preview 1");
    expect(imgs[1]).toHaveAttribute("alt", "Preview 2");
  });

  it("calls onRemove with correct index when remove button is clicked", async () => {
    const user = userEvent.setup();
    const images = [
      new Blob(["img1"], { type: "image/jpeg" }),
      new Blob(["img2"], { type: "image/png" }),
    ];

    render(<ImagePreview images={images} onRemove={mockOnRemove} />);

    const removeButtons = screen.getAllByRole("button");
    expect(removeButtons).toHaveLength(2);

    await user.click(removeButtons[1]);
    expect(mockOnRemove).toHaveBeenCalledWith(1);
  });

  it("calls onRemove with index 0 for first image remove button", async () => {
    const user = userEvent.setup();
    const images = [new Blob(["img1"], { type: "image/jpeg" })];

    render(<ImagePreview images={images} onRemove={mockOnRemove} />);

    await user.click(screen.getByRole("button"));
    expect(mockOnRemove).toHaveBeenCalledWith(0);
  });

  it("revokes object URLs on unmount", () => {
    const images = [new Blob(["img1"], { type: "image/jpeg" })];
    mockCreateObjectURL.mockReturnValue("blob:url-to-revoke");

    const { unmount } = render(
      <ImagePreview images={images} onRemove={mockOnRemove} />,
    );

    unmount();
    expect(mockRevokeObjectURL).toHaveBeenCalledWith("blob:url-to-revoke");
  });
});
