import BodyTypeSelect from "@/components/forms/BodyTypeSelect";
import DrivetrainToggleGroup from "@/components/forms/DrivetrainToggleGroup";
import FuelSelect from "@/components/forms/FuelSelect";
import MakeSelect from "@/components/forms/MakeSelect";
import ModelSelect from "@/components/forms/ModelSelect";
import TransmissionToggleGroup from "@/components/forms/TransmissionToggleGroup";
import { Button } from "@/components/ui/button";
import {
  Form,
  FormControl,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from "@/components/ui/form";
import { Input } from "@/components/ui/input";
import { cn } from "@/lib/utils";
import { zodResolver } from "@hookform/resolvers/zod";
import { useForm } from "react-hook-form";
import { CarFormSchema } from "../schemas/carFormSchema";
import { CarFormData } from "../types/CarFormData";

type CarFormProps = {
  car: CarFormData;
  onSubmit: (formData: CarFormData) => Promise<void>;
  className?: string;
};

const CarForm = ({ car, onSubmit, className }: CarFormProps) => {
  const form = useForm<CarFormData>({
    defaultValues: car,
    resolver: zodResolver(CarFormSchema),
  });

  const handleSubmit = async (formData: CarFormData) => {
    await onSubmit(formData);
    form.reset();
  };

  const selectedMake = form.watch("makeId");

  return (
    <div className={cn(className)}>
      <Form {...form}>
        <form
          className="grid w-full min-w-3xs grid-cols-4 space-y-4 gap-x-6 gap-y-6 md:gap-x-12 md:gap-y-8"
          onSubmit={form.handleSubmit(handleSubmit)}
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
              <FormItem className="col-span-2 flex flex-col justify-start">
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
          <Button type="submit">Confirm</Button>
        </form>
      </Form>
    </div>
  );
};

export default CarForm;
