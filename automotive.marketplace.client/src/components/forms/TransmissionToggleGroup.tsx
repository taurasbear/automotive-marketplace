import { getAllTransmissionsOptions } from "@/api/enum/getAllTransmissionsOptions";
import { toggleVariants } from "@/components/ui/toggle";
import { cn } from "@/lib/utils";
import * as ToggleGroupPrimitive from "@radix-ui/react-toggle-group";
import { useQuery } from "@tanstack/react-query";
import { type VariantProps } from "class-variance-authority";
import { ToggleGroup, ToggleGroupItem } from "../ui/toggle-group";

type TransmissionToggleGroupProps = React.ComponentProps<
  typeof ToggleGroupPrimitive.Root
> &
  VariantProps<typeof toggleVariants>;

const TransmissionToggleGroup = ({
  className,
  ...props
}: TransmissionToggleGroupProps) => {
  const { data: transmissionsQuery } = useQuery(getAllTransmissionsOptions);

  const transmissions = transmissionsQuery?.data || [];

  return (
    <div className={cn(className)}>
      <ToggleGroup {...props}>
        {transmissions.map((transmission) => (
          <ToggleGroupItem
            key={transmission.id}
            value={transmission.id}
            className="flex-none"
          >
            {transmission.name}
          </ToggleGroupItem>
        ))}
      </ToggleGroup>
    </div>
  );
};

export default TransmissionToggleGroup;
