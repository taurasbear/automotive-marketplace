import DrivetrainToggleGroup from "@/components/forms/DrivetrainToggleGroup";
import BodyTypeSelect from "@/components/forms/select/BodyTypeSelect";
import FuelSelect from "@/components/forms/select/FuelSelect";
import MakeSelect from "@/components/forms/select/MakeSelect";
import ModelSelect from "@/components/forms/select/ModelSelect";
import VariantTable from "@/components/forms/VariantTable";
import TransmissionToggleGroup from "@/components/forms/TransmissionToggleGroup";
import { Button } from "@/components/ui/button";
import { Checkbox } from "@/components/ui/checkbox";
import {
  Form,
  FormControl,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from "@/components/ui/form";
import { Input } from "@/components/ui/input";
import { Textarea } from "@/components/ui/textarea";
import { VALIDATION } from "@/constants/validation";
import { Variant } from "@/features/variantList/types/Variant";
import { zodResolver } from "@hookform/resolvers/zod";
import { Lock, Unlock } from "lucide-react";
import { useState } from "react";
import { useForm } from "react-hook-form";
import { useCreateListing } from "../api/useCreateListing";
import { CreateListingSchema } from "../schemas/createListingSchema";
import { CreateListingFormData } from "../types/CreateListingFormData";
import ImagePreview from "./ImagePreview";
import ImageUploadInput from "./ImageUploadInput";

type CreateListingFormProps = {
  className?: string;
};

const CreateListingForm = ({ className }: CreateListingFormProps) => {
  const [isModified, setIsModified] = useState(false);

  const form = useForm({
    resolver: zodResolver(CreateListingSchema),
    defaultValues: {
      price: 0,
      vin: "",
      powerKw: 0,
      engineSizeMl: 0,
      mileage: 0,
      isSteeringWheelRight: true,
      makeId: "",
      modelId: "",
      variantId: "",
      city: "",
      colour: "",
      isUsed: false,
      isCustom: true,
      year: undefined,
      transmissionId: "",
      fuelId: "",
      bodyTypeId: "",
      drivetrainId: "",
      doorCount: 0,
      images: [],
    },
  });

  const { mutateAsync: createListingAsync } = useCreateListing();

  const onSubmit = async (formData: CreateListingFormData) => {
    const { makeId, ...command } = formData;
    await createListingAsync({
      ...command,
      variantId: isModified ? undefined : command.variantId || undefined,
    });
    form.reset();
    setIsModified(false);
  };

  const selectedMake = form.watch("makeId");
  const selectedModelId = form.watch("modelId") ?? "";
  const variantId = form.watch("variantId") ?? "";
  const images = form.watch("images") ?? [];

  const specLocked = !!variantId && !isModified;

  const handleVariantSelect = (variant: Variant | null) => {
    if (variant !== null) {
      form.setValue("variantId", variant.id);
      form.setValue("isCustom", false);
      form.setValue("fuelId", variant.fuelId);
      form.setValue("transmissionId", variant.transmissionId);
      form.setValue("bodyTypeId", variant.bodyTypeId);
      form.setValue("doorCount", variant.doorCount);
      form.setValue("powerKw", variant.powerKw);
      form.setValue("engineSizeMl", variant.engineSizeMl);
      setIsModified(false);
    } else {
      form.setValue("variantId", "");
      form.setValue("isCustom", true);
      setIsModified(false);
    }
  };

  const handleIsModifiedChange = (checked: boolean) => {
    setIsModified(checked);
    form.setValue("isCustom", checked);
  };

  const handleRemoveImage = (index: number) => {
    const updated = images.filter((_, i) => i !== index);
    form.setValue("images", updated);
  };

  return (
    <div className={className}>
      <Form {...form}>
        <form
          onSubmit={form.handleSubmit(onSubmit)}
          className="grid w-full min-w-3xs grid-cols-1 gap-x-6 gap-y-6 md:grid-cols-3 md:gap-x-8 md:gap-y-8"
        >
          {/* Row 1: Make, Model, Year */}
          <FormField
            name="makeId"
            control={form.control}
            render={({ field }) => (
              <FormItem className="flex flex-col justify-start">
                <FormLabel>Car make*</FormLabel>
                <FormControl>
                  <MakeSelect
                    isAllMakesEnabled={false}
                    onValueChange={(value) => {
                      field.onChange(value);
                      form.setValue("modelId", "");
                      form.setValue("variantId", "");
                      form.setValue("isCustom", true);
                      setIsModified(false);
                    }}
                    {...field}
                  />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />
          <FormField
            name="modelId"
            control={form.control}
            render={({ field }) => (
              <FormItem className="flex flex-col justify-start">
                <FormLabel>Car model*</FormLabel>
                <FormControl>
                  <ModelSelect
                    isAllModelsEnabled={false}
                    disabled={!selectedMake}
                    onValueChange={(modelId) => {
                      form.setValue("modelId", modelId);
                      form.setValue("variantId", "");
                      form.setValue("isCustom", true);
                      setIsModified(false);
                    }}
                    selectedMake={selectedMake}
                    value={field.value}
                  />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />
          <FormField
            name="year"
            control={form.control}
            render={({ field }) => (
              <FormItem className="flex flex-col justify-start">
                <FormLabel>Year*</FormLabel>
                <FormControl>
                  <Input type="number" min={VALIDATION.YEAR.MIN} {...field} />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />

          {/* Row 2: VariantTable */}
          <div className="col-span-1 md:col-span-3">
            <VariantTable
              modelId={selectedModelId}
              selectedVariantId={variantId}
              onSelect={handleVariantSelect}
            />
          </div>

          {/* Row 3: Spec group */}
          <div className="col-span-1 rounded-lg border p-4 md:col-span-3">
            <div className="mb-4 flex items-center gap-3">
              {specLocked ? (
                <Lock className="text-muted-foreground h-4 w-4" />
              ) : (
                <Unlock className="text-muted-foreground h-4 w-4" />
              )}
              <span className="text-sm font-medium">
                {specLocked
                  ? "Specifications (locked from variant)"
                  : "Specifications"}
              </span>
              {!!variantId && (
                <label className="ml-auto flex cursor-pointer items-center gap-2 text-sm">
                  <Checkbox
                    checked={isModified}
                    onCheckedChange={(checked) =>
                      handleIsModifiedChange(checked === true)
                    }
                  />
                  Car is modified
                </label>
              )}
            </div>
            <div className="grid grid-cols-1 gap-4 md:grid-cols-3">
              <FormField
                name="fuelId"
                control={form.control}
                render={({ field }) => (
                  <FormItem className="flex flex-col justify-start">
                    <FormLabel>Fuel type*</FormLabel>
                    <FormControl>
                      <FuelSelect
                        disabled={specLocked}
                        onValueChange={field.onChange}
                        {...field}
                      />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />
              <FormField
                name="transmissionId"
                control={form.control}
                render={({ field }) => (
                  <FormItem className="flex flex-col justify-start">
                    <FormLabel>Transmission*</FormLabel>
                    <FormControl>
                      <TransmissionToggleGroup
                        type="single"
                        disabled={specLocked}
                        value={field.value ?? ""}
                        onValueChange={field.onChange}
                      />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />
              <FormField
                name="bodyTypeId"
                control={form.control}
                render={({ field }) => (
                  <FormItem className="flex flex-col justify-start">
                    <FormLabel>Body type*</FormLabel>
                    <FormControl>
                      <BodyTypeSelect
                        disabled={specLocked}
                        onValueChange={field.onChange}
                        {...field}
                      />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />
              <FormField
                name="powerKw"
                control={form.control}
                render={({ field }) => (
                  <FormItem className="flex flex-col justify-start">
                    <FormLabel>Engine power (kW)*</FormLabel>
                    <FormControl>
                      <Input
                        type="number"
                        min={0}
                        disabled={specLocked}
                        {...field}
                      />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />
              <FormField
                name="engineSizeMl"
                control={form.control}
                render={({ field }) => (
                  <FormItem className="flex flex-col justify-start">
                    <FormLabel>Engine size (ml)*</FormLabel>
                    <FormControl>
                      <Input
                        type="number"
                        min={0}
                        disabled={specLocked}
                        {...field}
                      />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />
              <FormField
                name="doorCount"
                control={form.control}
                render={({ field }) => (
                  <FormItem className="flex flex-col justify-start">
                    <FormLabel>Door count*</FormLabel>
                    <FormControl>
                      <Input type="number" disabled={specLocked} {...field} />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />
            </div>
          </div>

          {/* Row 5: Colour, VIN, Drivetrain */}
          <FormField
            name="colour"
            control={form.control}
            render={({ field }) => (
              <FormItem className="flex flex-col justify-start">
                <FormLabel>Colour</FormLabel>
                <FormControl>
                  <Input placeholder="Crimson" type="text" {...field} />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />
          <FormField
            name="vin"
            control={form.control}
            render={({ field }) => (
              <FormItem className="flex flex-col justify-start">
                <FormLabel>VIN</FormLabel>
                <FormControl>
                  <Input
                    placeholder="1G1JC524417418958"
                    type="text"
                    {...field}
                  />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />
          <FormField
            name="drivetrainId"
            control={form.control}
            render={({ field }) => (
              <FormItem className="flex flex-col justify-start">
                <FormLabel>Drivetrain*</FormLabel>
                <FormControl>
                  <DrivetrainToggleGroup
                    type="single"
                    value={field.value}
                    onValueChange={field.onChange}
                  />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />
          {/* Row 4: Price, Mileage, City */}
          <FormField
            name="price"
            control={form.control}
            render={({ field }) => (
              <FormItem className="flex flex-col justify-start">
                <FormLabel>Car price (€)*</FormLabel>
                <FormControl>
                  <Input type="number" min={0} {...field} />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />
          <FormField
            name="mileage"
            control={form.control}
            render={({ field }) => (
              <FormItem className="flex flex-col justify-start">
                <FormLabel>Mileage (km)*</FormLabel>
                <FormControl>
                  <Input type="number" min={0} {...field} />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />
          <FormField
            name="city"
            control={form.control}
            render={({ field }) => (
              <FormItem className="flex flex-col justify-start">
                <FormLabel>City*</FormLabel>
                <FormControl>
                  <Input placeholder="Kaunas" type="text" {...field} />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />

          {/* Row 6: Description (2 cols), Images (1 col) */}
          <FormField
            name="description"
            control={form.control}
            render={({ field }) => (
              <FormItem className="col-span-1 flex flex-col justify-start md:col-span-2">
                <FormLabel>Description</FormLabel>
                <FormControl>
                  <Textarea className="max-h-96" {...field} />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />
          <FormField
            name="images"
            control={form.control}
            render={({ field }) => (
              <FormItem className="flex flex-col justify-start">
                <FormLabel>Vehicle images*</FormLabel>
                <FormControl>
                  <ImageUploadInput field={field} />
                </FormControl>
                <ImagePreview images={images} onRemove={handleRemoveImage} />
                <FormMessage />
              </FormItem>
            )}
          />

          {/* Row 7: Steering wheel, Used car, Submit */}
          <FormField
            name="isSteeringWheelRight"
            control={form.control}
            render={({ field }) => (
              <FormItem className="flex flex-row items-center gap-2">
                <FormControl>
                  <Checkbox
                    checked={field.value}
                    onCheckedChange={field.onChange}
                    className="h-5 w-5"
                  />
                </FormControl>
                <FormLabel className="mt-0">Steering wheel on right</FormLabel>
                <FormMessage />
              </FormItem>
            )}
          />
          <FormField
            name="isUsed"
            control={form.control}
            render={({ field }) => (
              <FormItem className="flex flex-row items-center gap-2">
                <FormControl>
                  <Checkbox
                    checked={field.value}
                    onCheckedChange={field.onChange}
                    className="h-5 w-5"
                  />
                </FormControl>
                <FormLabel className="mt-0">Used car</FormLabel>
                <FormMessage />
              </FormItem>
            )}
          />
          <Button type="submit">Submit</Button>
        </form>
      </Form>
    </div>
  );
};

export default CreateListingForm;
