import BodyTypeSelect from "@/components/forms/BodyTypeSelect";
import DrivetrainToggleGroup from "@/components/forms/DrivetrainToggleGroup";
import FuelSelect from "@/components/forms/FuelSelect";
import MakeSelect from "@/components/forms/MakeSelect";
import ModelSelect from "@/components/forms/ModelSelect";
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
      power: 0,
      engineSize: 0,
      mileage: 0,
      isSteeringWheelRight: true,
      makeId: "",
      modelId: "",
      city: "",
      colour: "",
      isUsed: false,
      year: 0,
      transmission: "",
      fuel: "",
      bodyType: "",
      drivetrain: "",
      doorCount: 0,
      images: [],
    },
  });

  const { mutateAsync: createListingAsync } = useCreateListing();

  const onSubmit = async (formData: CreateListingFormData) => {
    await createListingAsync(formData);
    form.reset();
  };

  const selectedMake = form.watch("makeId");

  return (
    <div className={className}>
      <Form {...form}>
        <form
          onSubmit={form.handleSubmit(onSubmit)}
          className="grid w-full min-w-3xs grid-cols-2 space-y-4 gap-x-6 gap-y-6 md:grid-cols-6 md:gap-x-12 md:gap-y-12"
        >
          <FormField
            name="makeId"
            control={form.control}
            render={({ field }) => (
              <FormItem className="col-span-2 flex flex-col justify-start">
                <FormLabel>Car make*</FormLabel>
                <FormControl>
                  <MakeSelect
                    isAllMakesEnabled={false}
                    onValueChange={field.onChange}
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
              <FormItem className="col-span-2 flex flex-col justify-start">
                <FormLabel>Car model*</FormLabel>
                <FormControl>
                  <ModelSelect
                    isAllModelsEnabled={false}
                    disabled={!selectedMake}
                    onValueChange={field.onChange}
                    selectedMake={selectedMake}
                    {...field}
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
              <FormItem className="col-span-2 flex flex-col justify-start">
                <FormLabel>Year*</FormLabel>
                <FormControl>
                  <Input type="number" min={0} {...field} />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />
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
          <FormField
            name="power"
            control={form.control}
            render={({ field }) => (
              <FormItem className="col-span-2 flex flex-col justify-start">
                <FormLabel>Engine power (kw)*</FormLabel>
                <FormControl>
                  <Input type="number" min={0} {...field} />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />
          <FormField
            name="engineSize"
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
          <FormField
            name="transmission"
            control={form.control}
            render={({ field }) => (
              <FormItem className="col-span-2 flex flex-col justify-start">
                <FormLabel>Transmission type*</FormLabel>
                <FormControl>
                  <TransmissionToggleGroup
                    type="single"
                    onValueChange={field.onChange}
                    {...field}
                  />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />
          <FormField
            name="fuel"
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
            name="bodyType"
            control={form.control}
            render={({ field }) => (
              <FormItem className="col-span-1 flex flex-col justify-start">
                <FormLabel>Body type*</FormLabel>
                <FormControl>
                  <BodyTypeSelect onValueChange={field.onChange} {...field} />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />
          <FormField
            name="drivetrain"
            control={form.control}
            render={({ field }) => (
              <FormItem className="col-span-1 flex flex-col justify-start md:col-span-2">
                <FormLabel>Drivetrain*</FormLabel>
                <FormControl>
                  <DrivetrainToggleGroup
                    type="single"
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
