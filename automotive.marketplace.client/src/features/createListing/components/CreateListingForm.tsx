import DrivetrainToggleGroup from "@/components/forms/DrivetrainToggleGroup";
import BodyTypeSelect from "@/components/forms/select/BodyTypeSelect";
import FuelSelect from "@/components/forms/select/FuelSelect";
import MakeSelect from "@/components/forms/select/MakeSelect";
import ModelSelect from "@/components/forms/select/ModelSelect";
import VariantSelect from "@/components/forms/select/VariantSelect";
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
import { zodResolver } from "@hookform/resolvers/zod";
import { useForm } from "react-hook-form";
import { useCreateListing } from "../api/useCreateListing";
import { CreateListingSchema } from "../schemas/createListingSchema";
import { CreateListingFormData } from "../types/CreateListingFormData";
import ImageUploadInput from "./ImageUploadInput";

type CreateListingFormProps = {
  className?: string;
};

const CreateListingForm = ({ className }: CreateListingFormProps) => {
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
      variantId: command.variantId || undefined,
    });
    form.reset();
  };

  const selectedMake = form.watch("makeId");
  const selectedModelId = form.watch("modelId") ?? "";
  const selectedYear = form.watch("year");
  const isCustom = form.watch("isCustom") ?? true;
  const variantId = form.watch("variantId") ?? "";

  const handleVariantChange = (value: string) => {
    form.setValue("variantId", value);
    form.setValue("isCustom", !value);
  };

  const handleModelChange = (modelId: string) => {
    form.setValue("modelId", modelId);
    form.setValue("variantId", "");
    form.setValue("isCustom", true);
  };

  return (
    <div className={className}>
      <Form {...form}>
        <form
          onSubmit={form.handleSubmit(onSubmit)}
          className="grid w-full min-w-3xs grid-cols-2 space-y-4 gap-x-6 gap-y-6 md:grid-cols-6 md:gap-x-12 md:gap-y-12"
        >
          {/* Make */}
          <FormField
            name="makeId"
            control={form.control}
            render={({ field }) => (
              <FormItem className="col-span-2 flex flex-col justify-start">
                <FormLabel>Car make*</FormLabel>
                <FormControl>
                  <MakeSelect
                    isAllMakesEnabled={false}
                    onValueChange={(value) => {
                      field.onChange(value);
                      form.setValue("modelId", "");
                      form.setValue("variantId", "");
                      form.setValue("isCustom", true);
                    }}
                    {...field}
                  />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />

          {/* Model */}
          <FormField
            name="modelId"
            control={form.control}
            render={({ field }) => (
              <FormItem className="col-span-2 flex flex-col justify-start">
                <FormLabel>Car model*</FormLabel>
                <FormControl>
                  <ModelSelect
                    isAllModelsEnabled={false}
                    disabled={!selectedMake}
                    onValueChange={handleModelChange}
                    selectedMake={selectedMake}
                    value={field.value}
                  />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />

          {/* Variant */}
          <FormItem className="col-span-2 flex flex-col justify-start">
            <FormLabel>Variant</FormLabel>
            <FormControl>
              <VariantSelect
                modelId={selectedModelId}
                year={selectedYear || undefined}
                value={variantId}
                onValueChange={handleVariantChange}
                disabled={!selectedModelId}
              />
            </FormControl>
          </FormItem>

          {/* Year — always visible */}
          <FormField
            name="year"
            control={form.control}
            render={({ field }) => (
              <FormItem className="col-span-2 flex flex-col justify-start">
                <FormLabel>Year*</FormLabel>
                <FormControl>
                  <Input type="number" min={VALIDATION.YEAR.MIN} {...field} />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />

          {/* Spec fields — shown when custom */}
          {isCustom && (
            <>
              <FormField
                name="fuelId"
                control={form.control}
                render={({ field }) => (
                  <FormItem className="col-span-1 flex flex-col justify-start">
                    <FormLabel>Fuel type*</FormLabel>
                    <FormControl>
                      <FuelSelect onValueChange={field.onChange} {...field} />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />
              <FormField
                name="transmissionId"
                control={form.control}
                render={({ field }) => (
                  <FormItem className="col-span-1 flex flex-col justify-start">
                    <FormLabel>Transmission*</FormLabel>
                    <FormControl>
                      <TransmissionToggleGroup
                        type="single"
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
                  <FormItem className="col-span-1 flex flex-col justify-start">
                    <FormLabel>Body type*</FormLabel>
                    <FormControl>
                      <BodyTypeSelect
                        onValueChange={field.onChange}
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
                  <FormItem className="col-span-2 flex flex-col justify-start">
                    <FormLabel>Door count*</FormLabel>
                    <FormControl>
                      <Input type="number" {...field} />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />
              <FormField
                name="powerKw"
                control={form.control}
                render={({ field }) => (
                  <FormItem className="col-span-2 flex flex-col justify-start">
                    <FormLabel>Engine power (kW)*</FormLabel>
                    <FormControl>
                      <Input type="number" min={0} {...field} />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />
              <FormField
                name="engineSizeMl"
                control={form.control}
                render={({ field }) => (
                  <FormItem className="col-span-2 flex flex-col justify-start">
                    <FormLabel>Engine size (ml)*</FormLabel>
                    <FormControl>
                      <Input type="number" min={0} {...field} />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />
            </>
          )}

          {/* Drivetrain — always shown */}
          <FormField
            name="drivetrainId"
            control={form.control}
            render={({ field }) => (
              <FormItem className="col-span-2 flex flex-col justify-start">
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

          {/* Price */}
          <FormField
            name="price"
            control={form.control}
            render={({ field }) => (
              <FormItem className="col-span-2 flex flex-col justify-start">
                <FormLabel>Car price (€)*</FormLabel>
                <FormControl>
                  <Input type="number" min={0} {...field} />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />

          {/* Mileage */}
          <FormField
            name="mileage"
            control={form.control}
            render={({ field }) => (
              <FormItem className="col-span-2 flex flex-col justify-start">
                <FormLabel>Mileage (km)*</FormLabel>
                <FormControl>
                  <Input type="number" min={0} {...field} />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />

          {/* City */}
          <FormField
            name="city"
            control={form.control}
            render={({ field }) => (
              <FormItem className="col-span-2 flex flex-col justify-start">
                <FormLabel>City*</FormLabel>
                <FormControl>
                  <Input placeholder="Kaunas" type="text" {...field} />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />

          {/* Description */}
          <FormField
            name="description"
            control={form.control}
            render={({ field }) => (
              <FormItem className="col-span-2 flex flex-col justify-start md:col-span-4">
                <FormLabel>Description</FormLabel>
                <FormControl>
                  <Textarea className="max-h-96" {...field} />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />

          {/* Images */}
          <FormField
            name="images"
            control={form.control}
            render={({ field }) => (
              <FormItem className="col-span-2 flex flex-col justify-start">
                <FormLabel>Vehicle images*</FormLabel>
                <FormControl>
                  <ImageUploadInput field={field} />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />

          {/* VIN */}
          <FormField
            name="vin"
            control={form.control}
            render={({ field }) => (
              <FormItem className="col-span-2 flex flex-col justify-start">
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

          {/* Colour */}
          <FormField
            name="colour"
            control={form.control}
            render={({ field }) => (
              <FormItem className="col-span-2 flex flex-col justify-start">
                <FormLabel>Colour</FormLabel>
                <FormControl>
                  <Input placeholder="Crimson" type="text" {...field} />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />

          {/* Flags */}
          <FormField
            name="isSteeringWheelRight"
            control={form.control}
            render={({ field }) => (
              <FormItem className="col-span-1 flex flex-col items-center justify-start">
                <FormLabel>Steering wheel on right</FormLabel>
                <FormControl>
                  <Checkbox
                    checked={field.value}
                    onCheckedChange={field.onChange}
                    className="h-5 w-5"
                  />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />
          <FormField
            name="isUsed"
            control={form.control}
            render={({ field }) => (
              <FormItem className="col-span-1 flex flex-col items-center justify-evenly">
                <FormLabel>Used car</FormLabel>
                <FormControl>
                  <Checkbox
                    checked={field.value}
                    onCheckedChange={field.onChange}
                    className="h-5 w-5"
                  />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />

          <Button className="col-span-2 md:col-start-3" type="submit">
            Submit
          </Button>
        </form>
      </Form>
    </div>
  );
};

export default CreateListingForm;
