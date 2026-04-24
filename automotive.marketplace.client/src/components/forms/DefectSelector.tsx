import { getDefectCategoriesOptions } from "@/api/defect/getDefectCategoriesOptions";
import { useAddDefectImage } from "@/api/defect/useAddDefectImage";
import { useAddListingDefect } from "@/api/defect/useAddListingDefect";
import { useRemoveDefectImage } from "@/api/defect/useRemoveDefectImage";
import { useRemoveListingDefect } from "@/api/defect/useRemoveListingDefect";
import { Button } from "@/components/ui/button";
import { Checkbox } from "@/components/ui/checkbox";
import { Input } from "@/components/ui/input";
import { ListingDefectDto } from "@/features/listingDetails/types/GetListingByIdResponse";
import { getTranslatedName } from "@/lib/i18n/getTranslatedName";
import { useQuery } from "@tanstack/react-query";
import { Plus, X } from "lucide-react";
import { useRef, useState } from "react";
import { useTranslation } from "react-i18next";

export type FormDefect = {
  defectCategoryId?: string;
  customName?: string;
  images: File[];
};

type DefectSelectorFormProps = {
  mode: "form";
  selectedDefects: FormDefect[];
  onDefectsChange: (defects: FormDefect[]) => void;
};

type DefectSelectorApiProps = {
  mode: "api";
  listingId: string;
  existingDefects: ListingDefectDto[];
};

type DefectSelectorProps = DefectSelectorFormProps | DefectSelectorApiProps;

const DefectSelector = (props: DefectSelectorProps) => {
  const { t, i18n } = useTranslation("common");
  const [customDefectName, setCustomDefectName] = useState("");

  const { data: categoriesQuery } = useQuery(getDefectCategoriesOptions);
  const categories = categoriesQuery?.data || [];

  // Mutations for API mode
  const addDefectMutation = useAddListingDefect();
  const removeDefectMutation = useRemoveListingDefect();
  const addImageMutation = useAddDefectImage();
  const removeImageMutation = useRemoveDefectImage();

  const fileInputRefs = useRef<{ [key: string]: HTMLInputElement | null }>({});

  const selectedDefects =
    props.mode === "form" ? props.selectedDefects : props.existingDefects;

  const isDefectSelected = (categoryId: string) => {
    return selectedDefects.some(
      (defect) => defect.defectCategoryId === categoryId,
    );
  };

  const toggleDefectCategory = async (categoryId: string) => {
    if (props.mode === "form") {
      const isSelected = isDefectSelected(categoryId);
      if (isSelected) {
        // Remove defect
        const updatedDefects = props.selectedDefects.filter(
          (defect) => defect.defectCategoryId !== categoryId,
        );
        props.onDefectsChange(updatedDefects);
      } else {
        // Add defect
        const newDefect: FormDefect = {
          defectCategoryId: categoryId,
          images: [],
        };
        props.onDefectsChange([...props.selectedDefects, newDefect]);
      }
    } else {
      // API mode
      const isSelected = isDefectSelected(categoryId);
      if (isSelected) {
        // Remove defect
        const defectToRemove = props.existingDefects.find(
          (defect) => defect.defectCategoryId === categoryId,
        );
        if (defectToRemove) {
          await removeDefectMutation.mutateAsync({ id: defectToRemove.id });
        }
      } else {
        // Add defect
        await addDefectMutation.mutateAsync({
          listingId: props.listingId,
          defectCategoryId: categoryId,
        });
      }
    }
  };

  const addCustomDefect = async () => {
    if (!customDefectName.trim()) return;

    if (props.mode === "form") {
      const newDefect: FormDefect = {
        customName: customDefectName.trim(),
        images: [],
      };
      props.onDefectsChange([...props.selectedDefects, newDefect]);
    } else {
      await addDefectMutation.mutateAsync({
        listingId: props.listingId,
        customName: customDefectName.trim(),
      });
    }

    setCustomDefectName("");
  };

  const removeDefect = async (defect: FormDefect | ListingDefectDto) => {
    if (props.mode === "form") {
      const updatedDefects = props.selectedDefects.filter((d) => d !== defect);
      props.onDefectsChange(updatedDefects);
    } else {
      await removeDefectMutation.mutateAsync({
        id: (defect as ListingDefectDto).id,
      });
    }
  };

  const getDefectName = (defect: FormDefect | ListingDefectDto) => {
    if (defect.customName) return defect.customName;

    if (defect.defectCategoryId) {
      if (props.mode === "api" && "defectCategoryName" in defect) {
        return defect.defectCategoryName;
      }
      const category = categories.find((c) => c.id === defect.defectCategoryId);
      if (category) {
        return getTranslatedName(category.translations, i18n.language);
      }
    }

    return "Unknown defect";
  };

  const handleFileSelect = async (
    defect: FormDefect | ListingDefectDto,
    files: FileList | null,
  ) => {
    if (!files || files.length === 0) return;

    const file = files[0];

    if (props.mode === "form") {
      const formDefect = defect as FormDefect;
      if (formDefect.images.length >= 3) return;

      const updatedDefects = props.selectedDefects.map((d) =>
        d === defect ? { ...d, images: [...d.images, file] } : d,
      );
      props.onDefectsChange(updatedDefects);
    } else {
      const listingDefect = defect as ListingDefectDto;
      if (listingDefect.images.length >= 3) return;

      await addImageMutation.mutateAsync({
        listingDefectId: listingDefect.id,
        image: file,
      });
    }
  };

  const removeImage = async (
    defect: FormDefect | ListingDefectDto,
    imageIndex: number,
  ) => {
    if (props.mode === "form") {
      const formDefect = defect as FormDefect;
      const updatedImages = formDefect.images.filter(
        (_, index) => index !== imageIndex,
      );
      const updatedDefects = props.selectedDefects.map((d) =>
        d === defect ? { ...d, images: updatedImages } : d,
      );
      props.onDefectsChange(updatedDefects);

      // Revoke object URL to prevent memory leak
      const file = formDefect.images[imageIndex];
      if (file) {
        URL.revokeObjectURL(URL.createObjectURL(file));
      }
    } else {
      const listingDefect = defect as ListingDefectDto;
      const imageToRemove = listingDefect.images[imageIndex];
      if (imageToRemove) {
        // Note: This assumes the image ID is stored in the URL or there's a separate field
        // For now, we'll use the image URL as identifier - this might need adjustment based on API
        await removeImageMutation.mutateAsync({ id: imageToRemove.url });
      }
    }
  };

  const getDefectKey = (defect: FormDefect | ListingDefectDto): string => {
    if (props.mode === "api") {
      return (defect as ListingDefectDto).id;
    }
    return (
      defect.defectCategoryId || defect.customName || Math.random().toString()
    );
  };

  return (
    <div className="space-y-6">
      <div>
        <h3 className="mb-4 text-lg font-medium">
          {t("defectSelector.title")}
        </h3>

        {/* Category grid */}
        <div className="mb-4 grid grid-cols-2 gap-3">
          {categories.map((category) => (
            <div key={category.id} className="flex items-center space-x-2">
              <Checkbox
                id={category.id}
                checked={isDefectSelected(category.id)}
                onCheckedChange={() => toggleDefectCategory(category.id)}
              />
              <label
                htmlFor={category.id}
                className="cursor-pointer text-sm leading-none font-medium peer-disabled:cursor-not-allowed peer-disabled:opacity-70"
              >
                {getTranslatedName(category.translations, i18n.language)}
              </label>
            </div>
          ))}
        </div>

        {/* Custom defect input */}
        <div className="flex gap-2">
          <Input
            placeholder={t("defectSelector.customPlaceholder")}
            value={customDefectName}
            onChange={(e) => setCustomDefectName(e.target.value)}
            onKeyDown={(e) => {
              if (e.key === "Enter") {
                e.preventDefault();
                addCustomDefect();
              }
            }}
            className="flex-1"
          />
          <Button
            type="button"
            onClick={addCustomDefect}
            disabled={!customDefectName.trim()}
            variant="outline"
          >
            {t("defectSelector.addCustom")}
          </Button>
        </div>
      </div>

      {/* Selected defects section */}
      <div>
        {selectedDefects.length === 0 ? (
          <p className="text-muted-foreground text-sm">
            {t("defectSelector.noDefects")}
          </p>
        ) : (
          <div className="space-y-4">
            {selectedDefects.map((defect) => {
              const defectKey = getDefectKey(defect);
              const images =
                props.mode === "form"
                  ? (defect as FormDefect).images
                  : (defect as ListingDefectDto).images;

              return (
                <div
                  key={defectKey}
                  className="space-y-3 border-l-4 border-amber-500 pl-3"
                >
                  <div className="flex items-center justify-between">
                    <h4 className="font-medium">{getDefectName(defect)}</h4>
                    <Button
                      type="button"
                      variant="ghost"
                      size="sm"
                      onClick={() => removeDefect(defect)}
                      className="h-8 w-8 p-0"
                    >
                      <X className="h-4 w-4" />
                    </Button>
                  </div>

                  <div>
                    <div className="mb-2 flex items-center justify-between">
                      <span className="text-muted-foreground text-sm">
                        {t("defectSelector.photoCount", {
                          count: images.length,
                        })}
                      </span>
                      {images.length < 3 && (
                        <Button
                          type="button"
                          variant="outline"
                          size="sm"
                          onClick={() =>
                            fileInputRefs.current[defectKey]?.click()
                          }
                          className="h-8"
                        >
                          <Plus className="mr-1 h-4 w-4" />
                          {t("defectSelector.addPhoto")}
                        </Button>
                      )}
                    </div>

                    {/* Photo previews */}
                    {images.length > 0 && (
                      <div className="flex flex-wrap gap-2">
                        {images.map((image, index) => {
                          const imageSrc =
                            props.mode === "form"
                              ? URL.createObjectURL(image as File)
                              : (image as { url: string; altText: string }).url;

                          return (
                            <div key={index} className="relative">
                              <img
                                src={imageSrc}
                                alt={
                                  props.mode === "form"
                                    ? `Defect image ${index + 1}`
                                    : (image as { altText: string }).altText
                                }
                                className="h-16 w-16 rounded border object-cover"
                              />
                              <Button
                                type="button"
                                variant="destructive"
                                size="sm"
                                onClick={() => removeImage(defect, index)}
                                className="absolute -top-2 -right-2 h-6 w-6 rounded-full p-0"
                              >
                                <X className="h-3 w-3" />
                              </Button>
                            </div>
                          );
                        })}
                      </div>
                    )}

                    {/* Hidden file input */}
                    <input
                      ref={(el) => {
                        fileInputRefs.current[defectKey] = el;
                      }}
                      type="file"
                      accept="image/*"
                      onChange={(e) => handleFileSelect(defect, e.target.files)}
                      className="hidden"
                    />
                  </div>
                </div>
              );
            })}
          </div>
        )}
      </div>
    </div>
  );
};

export default DefectSelector;
