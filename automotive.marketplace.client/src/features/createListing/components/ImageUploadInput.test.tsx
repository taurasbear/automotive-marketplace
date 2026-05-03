import { render, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, it, expect, vi, beforeEach } from "vitest";
import ImageUploadInput from "./ImageUploadInput";

const { mockImageCompression } = vi.hoisted(() => ({
  mockImageCompression: vi.fn(),
}));

vi.mock("browser-image-compression", () => ({
  default: mockImageCompression,
}));

describe("ImageUploadInput", () => {
  const mockOnChange = vi.fn();
  const mockField = {
    value: [] as File[],
    onChange: mockOnChange,
    onBlur: vi.fn(),
    name: "images" as const,
    ref: vi.fn(),
  };

  beforeEach(() => {
    mockOnChange.mockReset();
    mockImageCompression.mockReset();
    mockImageCompression.mockImplementation(
      (file: File) =>
        new Promise((resolve) =>
          resolve(new File([file], `compressed-${file.name}`, { type: file.type })),
        ),
    );
  });

  it("renders file input with correct attributes", () => {
    const { container } = render(<ImageUploadInput field={mockField as any} />);

    const fileInput = container.querySelector('input[type="file"]') as HTMLInputElement;
    expect(fileInput).toBeInTheDocument();
    expect(fileInput).toHaveAttribute("type", "file");
    expect(fileInput).toHaveAttribute("multiple");
    expect(fileInput).toHaveAttribute("accept", "image/*");
  });

  it("compresses and calls field.onChange when files are selected", async () => {
    const user = userEvent.setup();
    render(<ImageUploadInput field={mockField as any} />);

    const fileInput = document.querySelector('input[type="file"]') as HTMLInputElement;
    const file1 = new File(["content1"], "photo1.jpg", { type: "image/jpeg" });
    const file2 = new File(["content2"], "photo2.png", { type: "image/png" });

    await user.upload(fileInput, [file1, file2]);

    await waitFor(() => {
      expect(mockImageCompression).toHaveBeenCalledTimes(2);
    });

    await waitFor(() => {
      expect(mockOnChange).toHaveBeenCalledTimes(1);
    });

    const calledWith = mockOnChange.mock.calls[0][0];
    expect(calledWith).toHaveLength(2);
  });

  it("appends to existing images when field already has values", async () => {
    const user = userEvent.setup();
    const existingFile = new File(["existing"], "existing.jpg", { type: "image/jpeg" });
    const fieldWithValue = { ...mockField, value: [existingFile] };

    render(<ImageUploadInput field={fieldWithValue as any} />);

    const fileInput = document.querySelector('input[type="file"]') as HTMLInputElement;
    const newFile = new File(["new"], "new.jpg", { type: "image/jpeg" });

    await user.upload(fileInput, [newFile]);

    await waitFor(() => {
      expect(mockOnChange).toHaveBeenCalledTimes(1);
    });

    const calledWith = mockOnChange.mock.calls[0][0];
    expect(calledWith).toHaveLength(2);
  });

  it("passes correct compression options to imageCompression", async () => {
    const user = userEvent.setup();
    render(<ImageUploadInput field={mockField as any} />);

    const fileInput = document.querySelector('input[type="file"]') as HTMLInputElement;
    const file = new File(["content"], "photo.jpg", { type: "image/jpeg" });

    await user.upload(fileInput, [file]);

    await waitFor(() => {
      expect(mockImageCompression).toHaveBeenCalledWith(file, {
        maxSizeMB: 0.4,
        useWebWorker: true,
        maxWidthOrHeight: 1920,
      });
    });
  });
});
