import { ENDPOINTS } from "@/constants/endpoints";
import axiosClient from "@/lib/axios/axiosClient";
import { GetBodyTypesResponse } from "@/types/enum/GetBodyTypesResponse";
import { queryOptions } from "@tanstack/react-query";
import { enumKeys } from "../queryKeys/enumKeys";

const getBodyTypes = () =>
  axiosClient.get<GetBodyTypesResponse[]>(ENDPOINTS.ENUM.GET_BODY_TYPES);

export const getBodyTypesOptions = queryOptions({
  queryKey: enumKeys.bodyTypes(),
  queryFn: () => getBodyTypes(),
});
